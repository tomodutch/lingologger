using Discord;
using LingoLogger.Data.Access;
using LingoLogger.Data.Models;
using LingoLogger.Discord.Bot.Services;
using LingoLogger.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot.InteractionHandlers;

public class ProfileService(ILogger<ProfileService> logger, LingoLoggerDbContext dbContext, ChartService chartService)
{
    private readonly ILogger<ProfileService> _logger = logger;
    private readonly LingoLoggerDbContext _dbContext = dbContext;
    private readonly ChartService _chartService = chartService;

    public async Task GetProfileAsync(IDiscordInteraction interaction)
    {
        await interaction.DeferAsync();
        try
        {
            var userId = interaction.User.Id;
            var today = DateTimeOffset.UtcNow;
            var logs = await _dbContext.Logs.Where(l => l.User.DiscordId == interaction.User.Id)
                .Where(l => l.CreatedAt.Date >= today.AddDays(-31).Date)
                .GroupBy(l => l.LogType)
                .Select(g => new
                {
                    logType = g.Key,
                    TotalMinutes = Math.Round(g.Sum(l => l.AmountOfSeconds) / 60.0),
                })
                .ToDictionaryAsync(x => x.logType, x => x.TotalMinutes);

            var embedBuilder = new EmbedBuilder();
            embedBuilder = embedBuilder
                .WithColor(Color.Blue)
                .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                .WithTitle($"{interaction.User.GlobalName}'s profile")
                .WithImageUrl("attachment://loading-chart.png")
                .WithCurrentTimestamp();
            if (logs.Count > 0)
            {
                embedBuilder = embedBuilder.WithDescription("Stats for the past 31 days");
                var totalMinutes = logs.Values.Sum();
                embedBuilder.AddField("Total", $"{totalMinutes:0.##} minutes");
                foreach (var log in logs)
                {
                    var t = log.Key switch
                    {
                        LogType.Audible => "Listened",
                        LogType.Readable => "Read",
                        LogType.Watchable => "Watched",
                        _ => "Other"
                    };

                    embedBuilder.AddField(t, $"{log.Value} minutes", inline: true);
                }
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Images", "loading-chart.png");
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            await interaction.FollowupWithFileAsync(stream, "loading-chart.png", embed: embedBuilder.Build());
            try
            {
                var chartStream = await _chartService.GenerateChartAsync(interaction);
                await interaction.ModifyOriginalResponseAsync((e) =>
                {
                    e.Embed = embedBuilder.WithImageUrl("attachment://chart.png").Build();
                    e.Attachments = new[] {
                        new FileAttachment(chartStream, "chart.png")
                    };
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("/profile could not generate chart", ex);
                var errorFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Images", "loading-chart-error.png");
                using var errorFileStream = new FileStream(errorFilePath, FileMode.Open, FileAccess.Read);
                await interaction.ModifyOriginalResponseAsync((e) =>
                {
                    e.Embed = embedBuilder.WithImageUrl("attachment://loading-chart-error.png").Build();
                    e.Attachments = new[] {
                        new FileAttachment(errorFileStream, "loading-chart-error.png")
                    };
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"/profile: {ex.Message}", ex);
            await interaction.FollowupAsync("An error occurred while fetching the profile. Please try again later.");
        }
    }
}