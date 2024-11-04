using Discord.Interactions;
using LingoLogger.Data.Access;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot;

public class LogInteraction : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<LogInteraction> _logger;
    private readonly LingoLoggerDbContext _dbContext;

    public LogInteraction(ILogger<LogInteraction> logger, LingoLoggerDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [SlashCommand("test-log", "sample log")]
    public async Task Ping()
    {
        await RespondAsync("pong");
    }
}