using Discord.Interactions;
using LingoLogger.Discord.Bot.InteractionHandlers;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot;

public class MilestonesInteraction(ILogger<MilestonesInteraction> logger, MilestonesInteractionHandler handler) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("milestones", "List all milestones")]
    public async Task GetMilestonesAsync()
    {
        await handler.GetMilestonesAsync(Context.Interaction);
    }
}