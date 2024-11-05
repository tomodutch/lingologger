using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using LingoLogger.Data.Models;
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
        try
        {
        await FollowupAsync("logged", embed: BuildCreatedLogEmbed(description));
        } catch(Exception ex) {
        _logger.LogError($"{ex.Message}", ex);
        }
        _logger.LogInformation($"Send response {DateTimeOffset.UtcNow}");
    }

    [SlashCommand("episodes", "Log a watching activity.")]
    public async Task LogEpisodes(
        [Summary("media", "Type of medium being watched (e.g., YouTube, anime).")] string medium,
        [Summary("title", "Title of the video or show.")] string title,
        [Summary("episodes", "Number of episodes watched.")] int episodes,
        [Summary("episode_length", "Length of each episode")] string episodeLength)
    {
        await DeferAsync();
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
        await RespondAsync(embed: BuildCreatedLogEmbed(description));
    }

    // Subcommand for logging watching
    [SlashCommand("watched", "Log a watching activity.")]
    public async Task LogWatching(
        [Summary("media", "Type of medium being watched (e.g., YouTube, anime).")] string medium,
        [Summary("time", "Time spent watching in minutes.")] string time,
        [Summary("title", "Title of the video or show.")] string title,
        [Summary("episodes", "Number of episodes watched.")] int? episodes = null,
        [Summary("episode_length", "Length of each episode")] string? episodeLength = null)
    {
        await DeferAsync();
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
        await RespondAsync(embed: BuildCreatedLogEmbed(description));
    }

    // Subcommand for logging listening
    [SlashCommand("listened", "Log a listening activity.")]
    public async Task LogListening(
        [Summary("media", "Type of media being listened to.")] string medium,
        [Summary("time", "Time spent listening.")] string time,
        [Summary("title", "Title of the audio.")] string title)
    {
        await DeferAsync();
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
        await RespondAsync(embed: BuildCreatedLogEmbed(description));
    }

    private Embed BuildCreatedLogEmbed(string description) {
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
