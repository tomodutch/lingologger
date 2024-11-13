using Discord.Interactions;
using LingoLogger.Discord.Bot.InteractionHandlers;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot;

public class LogsInteraction : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<LogsInteraction> _logger;
    private readonly LogService _service;

    public LogsInteraction(ILogger<LogsInteraction> logger, LogService service)
    {
        _logger = logger;
        _service = service;
    }

    [SlashCommand("logs", "show all logs")]
    public async Task Logs()
    {
        await _service.GetLogsAsync(Context.Interaction);
    }
}