using Discord;
using LingoLogger.Data.Access;
using LingoLogger.Data.Models;
using LingoLogger.Discord.Bot.Services;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot.InteractionHandlers;

public class TogglIntegrationHandler(ILogger<TogglIntegrationHandler> logger, UserService userService, LingoLoggerDbContext dbContext)
{
    public async Task CreateIntegrationAsync(IDiscordInteraction interaction)
    {
        await interaction.DeferAsync();
        using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            var user = await userService.GetOrCreateUserAsync(interaction.User.Id);
            var secret = Guid.NewGuid().ToString();
            var integration = new TogglIntegration()
            {
                WebhookSecret = secret,
                UserId = user.Id,
                User = user,
                IsVerified = false
            };
            user.TogglIntegrations.Add(integration);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            var embedBuilder = new EmbedBuilder();
            embedBuilder = embedBuilder
                .WithTitle("Configured Toggl integration")
                .WithDescription($@"
                # Configure your Toggle workspace
                1) In your browser open https://track.toggl.com/timer
                2) In your desired workspace navigate to Manage > Integrations
                3) In the top bar select **Webhooks**
                4) Click on **+ Create new webhook**
                5) Fill in the form. The name can be anything
                6) For events. Make sure to select all **Time Entry** events
                7) Url endpoint: https://lingologger.app/api/users/{user.Id}/toggl/{integration.Id}
                8) Secret: {secret}
                9) Click on **Add Webhook**
                ".Trim())
                .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                .WithImageUrl("https://media1.tenor.com/m/91qvdRGlL-IAAAAd/rb.gif")
                .WithCurrentTimestamp();
            await interaction.FollowupAsync(ephemeral: true, embed: embedBuilder.Build());
        }
        catch (Exception ex)
        {
            logger.LogError("installing toggl", ex);
            await interaction.FollowupAsync(ephemeral: true, embed: new EmbedBuilder()
            .WithColor(Color.Orange)
            .WithThumbnailUrl(interaction.User.GetAvatarUrl())
            .WithTitle("Error")
            .WithDescription("An error occurred while installing Toggl integration. Please try again later")
            .WithCurrentTimestamp()
            .WithImageUrl("https://media.giphy.com/media/v1.Y2lkPTc5MGI3NjExZzNtcWxlc2w4d3ozdXRjNjM3M3IwdjV3YW1qMjJ1anBqOXlkenBpciZlcD12MV9naWZzX3NlYXJjaCZjdD1n/uDIfs260heCkUagE76/giphy.gif")
            .Build());
            await transaction.RollbackAsync();
        }
    }
}