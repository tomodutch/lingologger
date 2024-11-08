using System.Text.RegularExpressions;
using Discord.Interactions;
using LingoLogger.Discord.Bot.InteractionHandlers;
using LingoLogger.Discord.Bot.InteractionParameters;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot;

[Group("log", "log")]
public class LogInteraction : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<LogInteraction> _logger;
    private readonly LogService _service;

    public LogInteraction(ILogger<LogInteraction> logger, LogService service)
    {
        _logger = logger;
        _service = service;
    }

    [SlashCommand("undo", "Undo most recent log")]
    public async Task Undo()
    {
        await _service.UndoMostRecentLogAsync(Context.Interaction);
    }

    // Subcommand for logging reading
    [SlashCommand("read", "Log a reading activity.")]
    public async Task LogReading(
        [Summary("medium", "Type of media read.")]
        [Choice("Book", "book")]
        [Choice("Visual Novel", "vn")]
        [Choice("News", "news")]
        [Choice("Other", "other")] string medium,
        [Summary("time", "Time spent watching in minutes.")] string time,
        [Summary("title", "Title of the book or material read.")] string title,
        [Summary("notes", "Additional notes about the reading.")] string? notes = null,
        [Summary("characters", "Total number of characters read.")] int? characters = null,
        [Summary("date", "Created a log in the past format is \"yesterday\" or YYYY-MM-DD (i.e: 2024-02-14)")] string? createdAt = null)
    {
        var param = new LogReadParameters() {
            Medium = medium,
            Title = title,
            Time = time,
            Characters = characters,
            Notes = notes,
            Date = createdAt
        };
        await _service.LogReadAsync(Context.Interaction,param);
    }

    [SlashCommand("episodes", "Log a watching activity.")]
    public async Task LogEpisodes(
        [Choice("Anime", "anime")]
        [Choice("Drama", "drama")]
        [Choice("Other", "other")]
        [Summary("media", "Type of medium being watched (e.g., Anime, drama).")] string medium,
        [Summary("title", "Title of the video or show.")] string title,
        [Summary("episodes", "Number of episodes watched.")] int episodes,
        [Summary("episode_length", "Length of each episode")] string episodeLength,
        [Summary("notes", "Additional notes about the reading.")] string? notes,
        [Summary("date", "Created a log in the past format is \"yesterday\" or YYYY-MM-DD (i.e: 2024-02-14)")] string? createdAt = null)

    {
        await _service.LogEpisodicAsync(
            Context.Interaction,
            medium,
            episodes,
            episodeLength,
            title,
            notes,
            createdAt);
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
        [Summary("title", "Title of the video or show.")] string title,
        [Summary("notes", "Additional notes about the reading.")] string? notes,
        [Summary("date", "Created a log in the past format is \"yesterday\" or YYYY-MM-DD (i.e: 2024-02-14)")] string? createdAt = null)
    {
        await _service.LogWatchableAsync(
            Context.Interaction,
            medium,
            time,
            title,
            notes,
            createdAt);
    }

    // Subcommand for logging listening
    [SlashCommand("listened", "Log a listening activity.")]
    public async Task LogListening(
        [Choice("Audiobook", "audiobook")]
        [Choice("Podcast", "podcast")]
        [Choice("Other", "other")]
        [Summary("media", "Type of media being listened to.")] string medium,
        [Summary("time", "Time spent listening.")] string time,
        [Summary("title", "Title of the audio.")] string title,
        [Summary("notes", "Additional notes about the reading.")] string? notes,
        [Summary("date", "Created a log in the past format is \"yesterday\" or YYYY-MM-DD (i.e: 2024-02-14)")] string? createdAt = null)
    {
        await _service.LogAudibleAsync(
            Context.Interaction,
            medium,
            time,
            title,
            notes,
            createdAt);
    }
}
