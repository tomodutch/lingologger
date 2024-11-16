using System.Text.RegularExpressions;
using Discord.Interactions;
using LingoLogger.Data.Models;
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
        [Summary("medium", "Type of media read."), Autocomplete(typeof(ReadingMediumAutocompleteHandler))] string medium,
        [Summary("time", "Time spent watching in minutes.")] string time,
        [Summary("title", "Title of the book or material read."), Autocomplete(typeof(BookAutocompleteHandler))] string title,
        [Summary("notes", "Additional notes about the reading.")] string? notes = null,
        [Summary("characters", "Total number of characters read.")] int? characters = null,
        [Summary("date", "Created a log in the past format is \"yesterday\" or YYYY-MM-DD (i.e: 2024-02-14)")] string? createdAt = null)
    {
        var param = new LogParameters()
        {
            LogType = LogType.Readable,
            Medium = medium,
            Title = title,
            Time = time,
            Characters = characters,
            Notes = notes,
            Date = createdAt
        };
        await _service.LogAsync(Context.Interaction, param);
    }

    // Subcommand for logging watching
    [SlashCommand("watched", "Log a watching activity.")]
    public async Task LogWatching(
        [Summary("medium", "Type of media listened to."), Autocomplete(typeof(WatchableMediumAutocompleteHandler))] string medium,
        [Summary("time", "Time spent watching in minutes.")] string time,
        [Summary("title", "Title of the video or show.")] string title,
        [Summary("notes", "Additional notes about the reading.")] string? notes,
        [Summary("date", "Created a log in the past format is \"yesterday\" or YYYY-MM-DD (i.e: 2024-02-14)")] string? createdAt = null)
    {
        var param = new LogParameters()
        {
            LogType = LogType.Watchable,
            Medium = medium,
            Title = title,
            Time = time,
            Notes = notes,
            Date = createdAt
        };
        await _service.LogAsync(Context.Interaction, param);
    }

    // Subcommand for logging listening
    [SlashCommand("listened", "Log a listening activity.")]
    public async Task LogListening(
        [Summary("medium", "Type of media listened to."), Autocomplete(typeof(AudibleMediumAutocompleteHandler))] string medium,
        [Summary("time", "Time spent listening.")] string time,
        [Summary("title", "Title of the audio.")] string title,
        [Summary("notes", "Additional notes about the reading.")] string? notes,
        [Summary("date", "Created a log in the past format is \"yesterday\" or YYYY-MM-DD (i.e: 2024-02-14)")] string? createdAt = null)
    {
        var param = new LogParameters()
        {
            LogType = LogType.Audible,
            Medium = medium,
            Title = title,
            Time = time,
            Notes = notes,
            Date = createdAt
        };
        await _service.LogAsync(Context.Interaction, param);
    }

    [SlashCommand("anki", "Log time spend in anki")]
    public async Task LogAnki(
        [Summary("time", "Time spent doing anki reviews.")] string time,
        [Summary("notes", "Additional notes about the reading.")] string? notes,
        [Summary("date", "Created a log in the past format is \"yesterday\" or YYYY-MM-DD (i.e: 2024-02-14)")] string? createdAt = null)
    {
        var param = new LogParameters()
        {
            LogType = LogType.Anki,
            Medium = "Anki",
            Title = "Anki",
            Time = time,
            Notes = notes,
            Date = createdAt
        };
        await _service.LogAsync(Context.Interaction, param);
    }

    [SlashCommand("wrote", "Log time spend writing")]
    public async Task LogWriting(
        [Summary("time", "Time spent writing.")] string time,
        [Summary("title", "Title.")] string title,
        [Summary("notes", "Additional notes about the reading.")] string? notes,
        [Summary("date", "Created a log in the past format is \"yesterday\" or YYYY-MM-DD (i.e: 2024-02-14)")] string? createdAt = null)
    {
        var param = new LogParameters()
        {
            LogType = LogType.Writing,
            Medium = "Other",
            Title = title,
            Time = time,
            Notes = notes,
            Date = createdAt
        };
        await _service.LogAsync(Context.Interaction, param);
    }
}
