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
                .Where(l => l.CreatedAt.Date >= today.AddDays(-7).Date)
                .GroupBy(l => l.LogType)
                .Select(g => new
                {
                    logType = g.Key,
                    TotalMinutes = Math.Round(g.Sum(l => l.AmountOfSeconds) / 60.0)
                })
                .ToDictionaryAsync(x => x.logType, x => x.TotalMinutes);

            var embedBuilder = new EmbedBuilder();
            embedBuilder = embedBuilder
                .WithColor(Color.Blue)
                .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                .WithTitle($"{interaction.User.GlobalName}'s profile")
                .WithDescription("Stats for the passed 7 days")
                .WithImageUrl("attachment://chart.png")
                .WithCurrentTimestamp();
            foreach (var log in logs)
            {
                var t = log.Key switch
                {
                    "Audible" => "Listened",
                    "Readable" => "Read",
                    "Watchable" => "Watched",
                    "Episodic" => "Episodes",
                    _ => log.Key
                };

                embedBuilder.AddField(t, $"{log.Value} minutes");
            }

            await interaction.FollowupAsync(embed: embedBuilder.Build());
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
            _logger.LogError($"/profile: {ex.Message}", ex);
            await interaction.FollowupAsync("An error occurred while fetching the profile. Please try again later.");
        }
    }
}