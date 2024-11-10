using Discord.Interactions;
using LingoLogger.Discord.Bot.InteractionHandlers;

namespace LingoLogger.Discord.Bot;

[Group("integrations", "Configure third party integrations")]
public class IntegrationsInteraction : InteractionModuleBase<SocketInteractionContext>
{
    [Group("toggl", "Configure integration with Toggl")]
    public class TogglInteraction(TogglIntegrationHandler handler) : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("install", "Configure lingologger to receive logs from Toggl")]
        public async Task InstallToggl()
        {
            await handler.CreateIntegrationAsync(Context.Interaction);
        }
    }
}