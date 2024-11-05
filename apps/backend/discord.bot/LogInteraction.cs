using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using LingoLogger.Data.Models.Stores;
using LingoLogger.Web.Models;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot;

[Group("log", "log")]
public class LogInteraction : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<LogInteraction> _logger;
    private readonly ILogStore _logStore;

    public LogInteraction(ILogger<LogInteraction> logger, ILogStore logStore)
    {
        _logger = logger;
        _logStore = logStore;
    }

    [SlashCommand("undo", "Undo most recent log")]
    public async Task Undo()
    {
        await DeferAsync();
        try
        {
            var userId = Context.User.Id;
            var log = await _logStore.UndoMostRecentLogAsync(userId);
            if (log == null)
            {
                await FollowupAsync("No logs found for this user");
            }
            else
            {
                var embedBuilder = new EmbedBuilder();
                embedBuilder = embedBuilder
                    .WithTitle("Log undone")
                    .WithColor(Color.Blue)
                    .WithDescription($"{log.CreatedAt}: {log.Time} {log.Title}")
                    .WithCurrentTimestamp();

                await FollowupAsync("Log undone", embed: embedBuilder.Build());
            }

        }
        catch (Exception ex)
        {
            _logger.LogError($"Undo: {ex.Message}", ex);
            await FollowupAsync("Could not undo most recent log. Please try again later.");
        }
    }

    // Subcommand for logging reading
    [SlashCommand("read", "Log a reading activity.")]
    public async Task LogReading(
        [Summary("medium", "Type of media read.")]
        [Choice("Book", "book")]
        [Choice("Visual Novel", "vn")]
        [Choice("Light Novel", "ln")]
        [Choice("News", "news")] string medium,
        [Summary("time", "Time spent watching in minutes.")] string time,
        [Summary("title", "Title of the book or material read.")] string title,
        [Summary("notes", "Additional notes about the reading.")] string? notes = null,
        [Summary("characters", "Total number of characters read.")] int? characters = null)
    {
        await DeferAsync();
        try
        {
            _logger.LogInformation($"Incoming readable {DateTimeOffset.UtcNow}");
            var log = new ApiReadableLog()
            {
                Medium = medium,
                Title = title,
                CharactersRead = characters,
                Time = time,
                Source = "Discord"
            };
            await _logStore.SaveLogAsync(log, new()
            {
                Source = SaveLogSource.Discord,
                DiscordId = Context.User.Id
            });
            var description = $"Title: {title}\nTime: {time}\nCharacters: {(characters.HasValue ? characters.Value.ToString() : "N/A")}\nNotes: {notes ?? "No notes provided."}";
            await FollowupAsync("logged", embed: BuildCreatedLogEmbed(description));
            _logger.LogInformation($"Send response {DateTimeOffset.UtcNow}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed logging readable: {ex.Message}", ex);
            await FollowupAsync("An error occurred while logging. Try again later");
        }
    }

    [SlashCommand("episodes", "Log a watching activity.")]
    public async Task LogEpisodes(
        [Choice("Anime", "anime")]
        [Choice("Drama", "drama")]
        [Choice("Other", "other")]
        [Summary("media", "Type of medium being watched (e.g., Anime, drama).")] string medium,
        [Summary("title", "Title of the video or show.")] string title,
        [Summary("episodes", "Number of episodes watched.")] int episodes,
        [Summary("episode_length", "Length of each episode")] string episodeLength)
    {
        await DeferAsync();
        try
        {
            var log = new ApiEpisodicLog()
            {
                Medium = medium,
                Title = title,
                Time = "",
                Source = "Discord",
                EpisodeLength = episodeLength,
                AmountOfEpisodes = episodes
            };
            await _logStore.SaveLogAsync(log, new()
            {
                Source = SaveLogSource.Discord,
                DiscordId = Context.User.Id
            });

            var description = "Watchable logged";
            await FollowupAsync(embed: BuildCreatedLogEmbed(description));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed logging episodic: {ex.Message}", ex);
            await FollowupAsync("An error occurred while logging. Try again later");
        }
    }

    // Subcommand for logging watching
    [SlashCommand("watched", "Log a watching activity.")]
    public async Task LogWatching(
        [Choice("Youtube", "youtube")]
        [Choice("Anime", "anime")]
        [Choice("Drama", "drama")]
        [Choice("Other", "other")]
        [Summary("media", "Type of medium being watched (e.g., YouTube, anime, drama, other).")] string medium,
        [Summary("time", "Time spent watching in minutes.")] string time,
        [Summary("title", "Title of the video or show.")] string title)
    {
        await DeferAsync();
        try
        {
            var log = new ApiWatchableLog()
            {
                Medium = medium,
                Title = title,
                Time = time,
                Source = "Discord"
            };
            await _logStore.SaveLogAsync(log, new()
            {
                Source = SaveLogSource.Discord,
                DiscordId = Context.User.Id
            });

            var description = "Watchable logged";
            await FollowupAsync(embed: BuildCreatedLogEmbed(description));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed logging watchable: {ex.Message}", ex);
            await FollowupAsync("An error occurred while logging. Try again later");
        }
    }

    // Subcommand for logging listening
    [SlashCommand("listened", "Log a listening activity.")]
    public async Task LogListening(
        [Summary("media", "Type of media being listened to.")] string medium,
        [Summary("time", "Time spent listening.")] string time,
        [Summary("title", "Title of the audio.")] string title)
    {
        await DeferAsync();
        try
        {
            var log = new ApiAudibleLog()
            {
                Medium = medium,
                Title = title,
                Time = time,
                Source = "Discord"
            };

            await _logStore.SaveLogAsync(log, new()
            {
                Source = SaveLogSource.Discord,
                DiscordId = Context.User.Id
            });

            var description = "Listening logged";
            await FollowupAsync(embed: BuildCreatedLogEmbed(description));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed logging audible: {ex.Message}", ex);
            await FollowupAsync("An error occurred while logging. Try again later");
        }
    }

    private Embed BuildCreatedLogEmbed(string description)
    {
        var embed = new EmbedBuilder()
            .WithTitle("Created log")
            .WithDescription(description)
            .WithColor(Color.Blue)
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithThumbnailUrl(Context.User.GetAvatarUrl())
            .WithFooter("Thank you for logging your reading activity!");
        return embed.Build();
    }
}
