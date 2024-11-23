using Discord.Interactions;
using LingoLogger.Data.Models;
using LingoLogger.Discord.Bot.InteractionHandlers;
using LingoLogger.Discord.Bot.InteractionParameters;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot;

[Group("logs", "Manage your logs.")]
public class LogsInteraction : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<LogsInteraction> _logger;
    private readonly LogService _service;

    public LogsInteraction(ILogger<LogsInteraction> logger, LogService service)
    {
        _logger = logger;
        _service = service;
    }

    [SlashCommand("history", "View your log history.")]
    public async Task Logs()
    {
        await _service.GetLogsAsync(Context.Interaction);
    }

    [SlashCommand("undo", "Undo most recent log entry.")]
    public async Task Undo()
    {
        await _service.UndoMostRecentLogAsync(Context.Interaction);
    }

    [SlashCommand("add", "Add a new activity or task to your log.")]
    public async Task Log(
        [Choice("Listening (audio-only content).", "Audible")]
        [Choice("Reading (text-only content).", "Readable")]
        [Choice("Watching (audio and visual content).", "Watchable")]
        [Choice("Other (all other activities).", "Other")]
        [Summary("type", "Type of activity (e.g., Listening, Reading).")] string type,
        [Summary("time", "Minutes spent on the activity.")] string time,
        [Summary("title", "Enter the title. Start with 'book.', 'vn.', or 'anime.' to search online for it"), Autocomplete(typeof(TitleAutocompleteHandler))] string title,
        [Summary("notes", "Additional notes about the activity.")] string? notes = null,
        [Summary("characters", "Total characters read (if reading).")] int? characters = null,
        [Summary("date", "Date of the log (e.g., \"yesterday\" or YYYY-MM-DD).")] string? createdAt = null)
    {
        var param = new LogParameters()
        {
            LogType = LogTypeConverter.ConvertStringToLogType(type),
            Title = title,
            Time = time,
            Characters = characters,
            Notes = notes,
            Date = createdAt
        };
        await _service.LogAsync(Context.Interaction, param);
    }

    [SlashCommand("export", "Export logs")]
    public async Task Export()
    {
        await _service.ExportAsync(Context.Interaction);
    }
}