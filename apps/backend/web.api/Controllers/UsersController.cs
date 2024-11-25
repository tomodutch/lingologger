using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    [HttpGet("{discordUserId}/stats")]
    public async Task<IActionResult> GetStats(ulong discordUserId, CancellationToken token)
    {
        var minDate = DateTimeOffset.UtcNow.AddDays(-7).Date;
        var logs = await dbContext.Logs
            .Where(l => l.User.DiscordId == discordUserId)
            .Where(l => l.CreatedAt.Date >= minDate)
            .GroupBy(l => new
            {
                l.LogType,
                Day = l.CreatedAt.Date
            })
            .Select(group => new
            {
                group.Key.LogType,
                group.Key.Day,
                TotalSeconds = group.Sum(l => l.AmountOfSeconds)
            })
            .ToListAsync(token);

        var distinctDates = logs.Select(l => l.Day).Distinct().OrderBy(d => d).ToList();
        var data = new Dictionary<string, List<double>>();
        foreach (var logType in logs.Select(l => l.LogType).Distinct())
        {
            var logTypeData = new List<double>();
            foreach (var date in distinctDates)
            {
                var logForDate = logs.FirstOrDefault(l => l.LogType == logType && l.Day == date);
                var totalSeconds = logForDate?.TotalSeconds ?? 0;
                if (totalSeconds > 0)
                {
                    var time = Math.Round(totalSeconds / 60.0, 2);
                    logTypeData.Add(time);
                }
                else
                {
                    logTypeData.Add(logForDate?.TotalSeconds ?? 0);
                }
            }

            data[LogTypeConverter.ConvertLogTypeToString(logType)] = logTypeData;
        }
        using var httpClient = httpClientFactory.CreateClient("ChartApiClient");
        var content = new BarChartRequest()
        {
            Title = "logs",
            Index = distinctDates.Select(d => d.ToString("MM-dd")).ToList(),
            Data = data,
            XAxisTitle = "",
            YAxisTitle = "Minutes",
            Theme = "White"
        };

        var jsonBody = System.Text.Json.JsonSerializer.Serialize(content);
        var body = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync("/generate_barchart", body, token);
        response.EnsureSuccessStatusCode();
        var imageStream = await response.Content.ReadAsStreamAsync();

        return File(imageStream, "image/jpeg");
    }

    [HttpPost("{userId}/toggl/{integrationId}")]
    public async Task<IActionResult> CreateTogglLog(string userId, string integrationId, [FromBody] JsonElement webhookJson, CancellationToken token)
    {
        logger.LogInformation($"Incoming request. UserId: {userId}, integrationId: {integrationId}");
        if (!Guid.TryParse(userId, out var userGuid))
        {
            logger.LogInformation($"Incoming request body has malformed user id: {userId}");
            return BadRequest(new ApiResponse(false, "Malformed request. user id is not a uuid", new { }));
        }

        if (!Guid.TryParse(integrationId, out var integrationGuid))
        {
            logger.LogInformation($"Incoming request body has malformed integration id: {integrationId}");
            return BadRequest(new ApiResponse(false, "Malformed request. integration id is not a uuid", new { }));
        }

        var payloadProperty = GetJsonPayloadProperty(webhookJson);
        if (!payloadProperty.HasValue)
        {
            logger.LogInformation($"Incoming request has malformed payload");
            return BadRequest(new ApiResponse(false, "Malformed request. Payload expected", new { }));
        }

        var integration = await dbContext.TogglIntegrations
            .Where(t => t.UserId == userGuid)
            .Where(t => t.Id == integrationGuid)
            .Include(t => t.User)
            .FirstOrDefaultAsync(token);
        if (integration == null || integration?.User == null)
        {
            logger.LogInformation("No integration or user found");
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
                    logger.LogInformation("Attempt validating toggl");
                    if (await SendValidationRequestAsync(validationCodeUri, token))
                    {
                        integration.IsVerified = true;
                        logger.LogInformation($"Integration {integrationId} is now validated");
                        await dbContext.SaveChangesAsync(token);
                    }
                }
            }
            else if (payloadProperty.Value.ValueKind == JsonValueKind.Object)
            {
                var payload = TryDeserializePayload<TimeEntryPayload>(payloadProperty.Value);
                if (payload != null && payload.Stop.HasValue)
                {
                    var logType = GetLogType(payload.Tags);
                    var duration = (payload.Stop - payload.Start).Value.TotalSeconds;
                    var log = await dbContext.Logs
                        .Where(l => l.UserId == integration.UserId)
                        .Where(l => l.Source == "Toggl")
                        .Where(l => l.SourceEventId == payload.Id.ToString())
                        .FirstOrDefaultAsync(token);
                    if (log != null)
                    {
                        log.Title = payload.Description;
                        log.LogType = logType;
                        log.CreatedAt = payload.Stop.Value;
                        log.AmountOfSeconds = (int)duration;
                        logger.LogInformation($"Integration {integrationId} updated log");
                    }
                    else
                    {
                        var newLog = new Log()
                        {
                            Title = payload.Description,
                            UserId = integration.User.Id,
                            LogType = logType,
                            User = integration.User,
                            CreatedAt = payload.Stop.Value,
                            AmountOfSeconds = (int)duration,
                            Source = "Toggl",
                            SourceEventId = payload.Id.ToString()
                        };

                        await dbContext.Logs.AddAsync(newLog, token);
                        logger.LogInformation($"Integration {integrationId} added log");
                    }

                    await dbContext.SaveChangesAsync(token);
                }
            }

            await transaction.CommitAsync(token);
            return Ok(new ApiResponse(success: true, message: "Webhook processed", data: new { }));
        }
        catch (Exception ex)
        {
            logger.LogError("An error occurred while processing the webhook", ex);
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

    private LogType GetLogType(List<string> tags)
    {
        var mapping = new Dictionary<string, LogType>()
        {
            ["reading"] = LogType.Readable,
            ["listening"] = LogType.Audible,
            ["watching"] = LogType.Watchable,
        };
        foreach (var tag in tags)
        {
            if (mapping.TryGetValue(tag, out var foundLogType))
            {
                return foundLogType;
            }
        }

        return LogType.Other;
    }
}

public class ApiResponse(bool success, string message, object? data = null)
{
    public bool Success { get; set; } = success;
    public string Message { get; set; } = message;
    public object? Data { get; set; } = data;
}

public class BarChartRequest
{
    [JsonPropertyName("data")]
    public required Dictionary<string, List<double>> Data { get; set; }
    [JsonPropertyName("index")]
    public required IEnumerable<string> Index { get; set; }
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("xAxisTitle")]
    public required string XAxisTitle { get; set; }
    [JsonPropertyName("yAxisTitle")]
    public required string YAxisTitle { get; set; }
    [JsonPropertyName("theme")]
    public string Theme { get; internal set; }
}