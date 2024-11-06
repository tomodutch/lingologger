using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot.Services;

public class ChartService(ILogger<ChartService> logger, HttpClient httpClient)
{
    private readonly ILogger<ChartService> _logger = logger;
    private readonly HttpClient _httpClient = httpClient;

    public async Task<Stream> GenerateChartAsync()
    {
        try
        {
            var content = new BarChartRequest()
            {
                Title = "my chart",
                Index = ["2024-11-01", "2024-11-02", "2024-11-03"],
                Data = new()
                {
                    ["Reading"] = [1, 2, 3]
                }
            };
            var jsonBody = JsonSerializer.Serialize(content);
            var body = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/generate_barchart", body);
            response.EnsureSuccessStatusCode();
            var imageStream = await response.Content.ReadAsStreamAsync();
            return imageStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while calling the Chart API.");
            throw;
        }
    }
}

public class BarChartRequest
{
    [JsonPropertyName("data")]
    public required Dictionary<string, int[]> Data { get; set; }
    [JsonPropertyName("index")]
    public required IEnumerable<string> Index { get; set; }
    [JsonPropertyName("title")]
    public required string Title { get; set; }
}
