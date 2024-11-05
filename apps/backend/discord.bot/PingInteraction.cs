using Discord.Interactions;

namespace LingoLogger.Discord.Bot;

public class PingInteraction : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Echo an input")]
    public async Task Ping()
    {
        try
        {
            await DeferAsync();
        } catch (Exception ex) {
            ex.ToString();
        }
        await FollowupAsync("pong");
    }
}