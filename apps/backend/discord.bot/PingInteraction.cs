using Discord.Interactions;

namespace LingoLogger.Discord.Bot;

public class PingInteraction : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Echo an input")]
    public async Task Ping()
    {
        await RespondAsync("pong");
    }
}