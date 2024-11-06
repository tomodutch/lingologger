using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Discord;
using LingoLogger.Data.Access;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot.Services;

public class ChartService(ILogger<ChartService> logger, HttpClient httpClient, LingoLoggerDbContext dbContext)
{
    private readonly ILogger<ChartService> _logger = logger;
    private readonly HttpClient _httpClient = httpClient;
    private readonly LingoLoggerDbContext _dbContext = dbContext;

    public async Task<Stream> GenerateChartAsync(IDiscordInteraction interaction)
    {
        var minDate = DateTimeOffset.UtcNow.AddDays(-7).Date;
        var logs = await _dbContext.Logs
            .Where(l => l.User.DiscordId == interaction.User.Id)
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
            .ToListAsync();
        try
        {
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

                data[logType] = logTypeData;
            }

            var content = new BarChartRequest()
            {
                Title = $"{interaction.User.GlobalName}'s logs",
                Index = distinctDates.Select(d => d.ToString("MM-dd")).ToList(),
                Data = data,
                XAxisTitle = "",
                YAxisTitle = "Minutes"
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
    public required Dictionary<string, List<double>> Data { get; set; }
    [JsonPropertyName("index")]
    public required IEnumerable<string> Index { get; set; }
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("xAxisTitle")]
    public required string XAxisTitle { get; set; }
    [JsonPropertyName("yAxisTitle")]
    public required string YAxisTitle { get; set; }
}
