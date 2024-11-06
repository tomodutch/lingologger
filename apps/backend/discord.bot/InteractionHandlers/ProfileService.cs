using Discord;
using LingoLogger.Data.Access;
using LingoLogger.Data.Models;
using LingoLogger.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot.InteractionHandlers;

public class ProfileService(ILogger<ProfileService> logger, LingoLoggerDbContext dbContext)
{
    private readonly ILogger<ProfileService> _logger = logger;
    private readonly LingoLoggerDbContext _dbContext = dbContext;

    public async Task GetProfileAsync(IDiscordInteraction interaction)
    {
        await interaction.DeferAsync();
        try
        {
            var userId = interaction.User.Id;
            var today = DateTimeOffset.UtcNow;
            var logs = await _dbContext.Logs.Where(l => l.User.DiscordId == interaction.User.Id)
                .Where(l => l.CreatedAt.UtcDateTime.Year == today.Year && l.CreatedAt.UtcDateTime.DayOfYear == today.DayOfYear)
                .ToListAsync();

            var profile = new ApiProfile()
            {
                ReadTimeFormatted = "0s",
                ReadTimeInSeconds = 0,
                ListenTimeFormatted = "0s",
                ListenTimeInSeconds = 0,
                WatchTimeFormatted = "0s",
                WatchTimeInSeconds = 0,
                EpisodesWatched = 0
            };

            foreach (var log in logs)
            {
                switch (log)
                {
                    case ReadableLog readableLog:
                        profile.ReadTimeInSeconds += readableLog.AmountOfSeconds;
                        break;
                    case AudibleLog audibleLog:
                        profile.ListenTimeInSeconds += audibleLog.AmountOfSeconds;
                        break;
                    case WatchableLog watchableLog:
                        profile.WatchTimeInSeconds += watchableLog.AmountOfSeconds;
                        break;
                    case EpisodicLog episodicLog:
                        profile.WatchTimeInSeconds += episodicLog.AmountOfSeconds;
                        profile.EpisodesWatched += episodicLog.Episodes;
                        break;
                    default:
                        _logger.LogWarning($"{log.GetType()}: not supported in profile.");
                        break;
                }
            }
            var timeParser = new TimeParser();
            profile.ReadTimeFormatted = timeParser.SecondsToTimeFormat(profile.ReadTimeInSeconds);
            profile.ListenTimeFormatted = timeParser.SecondsToTimeFormat(profile.ListenTimeInSeconds);
            profile.WatchTimeFormatted = timeParser.SecondsToTimeFormat(profile.WatchTimeInSeconds);
            if (profile == null)
            {
                await interaction.FollowupAsync("No logs found.");
            }
            else
            {
                var embedBuilder = new EmbedBuilder();
                embedBuilder = embedBuilder
                    .WithColor(Color.Blue)
                    .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                    .WithTitle($"{interaction.User.GlobalName}'s profile")
                    .WithDescription("Stats for today")
                    .AddField("Reading", profile.ReadTimeFormatted)
                    .AddField("Listening", profile.ListenTimeFormatted)
                    .AddField("Watching", profile.WatchTimeFormatted)
                    .AddField("Episodes watched", profile.EpisodesWatched)
                    .WithCurrentTimestamp();
                await interaction.FollowupAsync(embed: embedBuilder.Build());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"/profile: {ex.Message}", ex);
            await interaction.FollowupAsync("An error occurred while fetching the profile. Please try again later.");
        }
    }
}