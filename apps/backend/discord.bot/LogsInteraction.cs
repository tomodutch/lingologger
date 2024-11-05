using System.Text;
using Discord;
using Discord.Interactions;
using LingoLogger.Data.Models.Stores;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot;

public class LogsInteraction : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<LogsInteraction> _logger;
    private readonly ILogStore _logStore;

    public LogsInteraction(ILogger<LogsInteraction> logger, ILogStore logStore)
    {
        _logger = logger;
        _logStore = logStore;
    }

    [SlashCommand("logs", "show all logs")]
    public async Task Logs()
    {
        await DeferAsync();
        try
        {
            var userId = Context.Interaction.User.Id;
            var logs = await _logStore.GetLogsAsync(userId);
            var embedBuilder = new EmbedBuilder();
            if (logs == null || logs.Any() == false)
            {
                embedBuilder = embedBuilder
                    .WithTitle("Logs")
                    .WithColor(Color.Orange)
                    .WithDescription("No logs found for user");
            }
            else
            {
                embedBuilder = embedBuilder.WithTitle("Logs").WithColor(Color.Blue);
                var sb = new StringBuilder();
                foreach (var log in logs)
                {
                    sb.Append($"{log.CreatedAt}: {log.Time} {log.Medium} {log.Title}  \n");
                }

                embedBuilder.WithDescription(sb.ToString());
            }
            await FollowupAsync("/logs", embed: embedBuilder.Build());
        }
        catch (Exception ex)
        {
            _logger.LogError($"/logs: {ex.Message}", ex);
            await FollowupAsync("An error occurred while fetching the logs. Please try again later.");
        }
    }
}