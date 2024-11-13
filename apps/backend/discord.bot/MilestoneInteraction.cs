using Discord.Interactions;
using LingoLogger.Discord.Bot.InteractionHandlers;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot;

[Group("milestone", "Sets milestones")]
public class MilestoneInteraction(ILogger<MilestoneInteraction> logger, MilestonesInteractionHandler handler) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("create", "Sets a new milestone")]
    public async Task CreateMilestoneAsync(
        [Summary("title", "The title of the new milestone")] string title
    )
    {
        await handler.CreateMilestoneAsync(Context.Interaction, title);
    }

    [SlashCommand("delete", "Delete a milestone")]
    public async Task DeleteMilestoneAsync(
        [Summary("title", "The title of the milestone to delete")] string title
    )
    {
        await handler.DeleteMilestoneAsync(Context.Interaction, title);

    }

    [SlashCommand("reached", "Mark a milestone as reached")]
    public async Task ReachMilestoneAsync(
        [Summary("title", "The title of the milestone that has been reached")] string title

    )
    {
        await handler.ReachMilestoneAsync(Context.Interaction, title, DateTimeOffset.UtcNow);
    }
}