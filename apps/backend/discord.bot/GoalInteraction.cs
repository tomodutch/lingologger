using Discord.Interactions;
using LingoLogger.Discord.Bot.InteractionHandlers;

namespace LingoLogger.Discord.Bot;

[Group("goals", "Goals")]
public class GoalInteraction(GoalService goalService) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly GoalService _goalService = goalService;

    [SlashCommand("all", "List all goals")]
    public async Task GetGoals()
    {
        await _goalService.GetGoalsAsync(Context.Interaction);
    }

    [SlashCommand("new", "Create a new goal")]
    public async Task CreateGoal([Summary("time", "Target")] string time, [Summary("until", "Until when")] string until)
    {
        await _goalService.CreateGoalAsync(Context.Interaction, new()
        {
            TargetTime = time,
            Until = until
        });
    }
}