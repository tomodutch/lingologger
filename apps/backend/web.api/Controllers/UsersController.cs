using System.Text.Json;
using LingoLogger.Data.Access;
using LingoLogger.Data.Models;
using LingoLogger.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace LingoLogger.Web.Api.Controllers;

[Route("api/users")]
public class UsersController(ILogger<UsersController> logger, LingoLoggerDbContext dbContext, IHttpClientFactory httpClientFactory) : ControllerBase
{
    [HttpPost("{userId}/toggl/{integrationId}")]
    public async Task<IActionResult> CreateTogglLog(string userId, string integrationId, [FromBody] JsonElement webhookJson, CancellationToken token)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            return BadRequest(new ApiResponse(false, "Malformed request. user id is not a uuid", new { }));
        }

        if (!Guid.TryParse(integrationId, out var integrationGuid))
        {
            return BadRequest(new ApiResponse(false, "Malformed request. integration id is not a uuid", new { }));
        }

        var payloadProperty = GetJsonPayloadProperty(webhookJson);
        if (!payloadProperty.HasValue)
        {
            return BadRequest(new ApiResponse(false, "Malformed request. Payload expected", new { }));
        }

        var integration = await dbContext.TogglIntegrations
            .Where(t => t.UserId == userGuid)
            .Where(t => t.Id == integrationGuid)
            .Include(t => t.User)
            .FirstOrDefaultAsync(token);

        if (integration == null || integration?.User == null)
        {
            return Ok(new ApiResponse(success: true, message: "Webhook processed", data: new { }));
        }

        using var transaction = await dbContext.Database.BeginTransactionAsync(token);
        try
        {
            if (payloadProperty.Value.ValueKind == JsonValueKind.String)
            {
                var validationCodeUri = GetValidationUrl(webhookJson);
                if (validationCodeUri != null)
                {
                    if (await SendValidationRequestAsync(validationCodeUri, token))
                    {
                        integration.IsVerified = true;
                        await dbContext.SaveChangesAsync(token);
                    }
                }
            }
            else if (payloadProperty.Value.ValueKind == JsonValueKind.Object)
            {
                var payload = TryDeserializePayload<TimeEntryPayload>(payloadProperty.Value);
                if (payload != null)
                {
                    if (payload.Tags.Contains("read") && payload.Stop.HasValue)
                    {
                        var duration = (payload.Stop - payload.Start).Value.TotalSeconds;
                        var log = await dbContext.Logs
                            .Where(l => l.UserId == integration.UserId)
                            .Where(l => l.Source == "Toggl")
                            .Where(l => l.SourceEventId == payload.Id.ToString())
                            .FirstOrDefaultAsync(token);

                        if (log == null)
                        {
                            log = new ReadableLog()
                            {
                                Title = payload.Description,
                                UserId = integration.User.Id,
                                User = integration.User,
                                Medium = "Book",
                                CreatedAt = payload.Stop.Value,
                                AmountOfSeconds = (int)duration,
                                Source = "Toggl",
                                SourceEventId = payload.Id.ToString()
                            };
                            await dbContext.Logs.AddAsync(log);
                        }
                        else
                        {
                            log.Title = payload.Description;
                            log.CreatedAt = payload.Stop.Value;
                            log.AmountOfSeconds = (int)duration;
                        }

                        await dbContext.SaveChangesAsync(token);
                    }

                }

            }

            await transaction.CommitAsync(token);
            return Ok(new ApiResponse(success: true, message: "Webhook processed", data: new { }));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(token);
            var problemDetails = new ProblemDetails()
            {
                Status = 500,
                Title = "An error occurred",
                Detail = "An error occurred while processing the webhook"
            };
            return Problem(
                detail: problemDetails.Detail,
                statusCode: problemDetails.Status,
                title: problemDetails.Title
            );
        }
    }

    private string? GetValidationUrl(JsonElement webhookJson)
    {
        // ping
        if (webhookJson.TryGetProperty("validation_code_url", out JsonElement validationCodeUrlElement))
        {
            var uriString = validationCodeUrlElement.GetString();
            if (uriString != null)
            {
                return uriString;
            }
        }

        return null;
    }

    private JsonElement? GetJsonPayloadProperty(JsonElement webhookJson)
    {
        if (webhookJson.TryGetProperty("payload", out JsonElement payloadElement))
        {
            return payloadElement;
        }

        return null;
    }

    private T? TryDeserializePayload<T>(JsonElement dynamic)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(dynamic.ToString());
        }
        catch (Exception ex)
        {
            logger.LogError("Could not deserialize payload", ex);
        }

        return default;
    }

    private async Task<bool> SendValidationRequestAsync(string validationCodeUrl, CancellationToken token)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.GetAsync(validationCodeUrl, token);
            response.EnsureSuccessStatusCode();

            logger.LogInformation("Validation request successful.");
            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error during validation request to {Url}", validationCodeUrl);
            return false;
        }
    }
}

public class ApiResponse(bool success, string message, object? data = null)
{
    public bool Success { get; set; } = success;
    public string Message { get; set; } = message;
    public object? Data { get; set; } = data;
}