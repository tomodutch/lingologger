using System.Text;
using Discord;
using Discord.WebSocket;
using LingoLogger.Data.Access;
using LingoLogger.Data.Models;
using LingoLogger.Discord.Bot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot.InteractionHandlers;

public class MilestonesInteractionHandler(ILogger<MilestonesInteractionHandler> logger, LingoLoggerDbContext dbContext, UserService userService)
{
    public async Task GetMilestonesAsync(SocketInteraction interaction)
    {
        await interaction.DeferAsync();
        var milestones = await dbContext.Milestones
            .Where(m => m.User.DiscordId == interaction.User.Id)
            .OrderBy(m => m.CreatedAt)
            .Select(m => m.ReachedAt.HasValue ? $"~~{m.Title}~~ reached at {m.ReachedAt.GetValueOrDefault():yyyy-MM-dd}" : $"- {m.Title}")
            .ToListAsync();

        if (milestones.Count == 0)
        {
            await interaction.FollowupAsync(embed: new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithTitle("No milestones found")
            .WithDescription("Could not find any milestones. Create one with the `/milestone create title:` command")
            .WithThumbnailUrl(interaction.User.GetAvatarUrl())
            .WithCurrentTimestamp()
            .Build());
        }
        else
        {
            await interaction.FollowupAsync(embed: new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithTitle($"{interaction.User.GlobalName}'s Milestones")
                .WithDescription(string.Join("\n", milestones))
                .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                .WithCurrentTimestamp()
                .Build());
        }
    }


    public async Task CreateMilestoneAsync(SocketInteraction interaction, string title)
    {
        await interaction.DeferAsync();
        using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            var user = await userService.GetOrCreateUserAsync(interaction.User.Id);
            var milestone = new Milestone()
            {
                Title = title,
            };
            user.Milestones.Add(milestone);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            await interaction.FollowupAsync(embed: new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithTitle($"Created milestone \"{title}\"")
                .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                .WithCurrentTimestamp()
                .Build());
        }
        catch (Exception ex)
        {
            logger.LogError($"Creating milestone failed: {ex.Message}", ex);
            await transaction.RollbackAsync();
            await interaction.FollowupAsync(embed: new EmbedBuilder()
                .WithColor(Color.Orange)
                .WithTitle("An error occurred")
                .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                .WithCurrentTimestamp()
                .Build());
        }
    }

    public async Task DeleteMilestoneAsync(SocketInteraction interaction, string title)
    {
        await interaction.DeferAsync();
        try
        {
            var milestone = await dbContext.Milestones
                .OrderBy(m => m.CreatedAt)
                .Where(m => m.User.DiscordId == interaction.User.Id && m.Title == title)
                .FirstOrDefaultAsync();
            if (milestone == null)
            {
                await interaction.FollowupAsync(embed: new EmbedBuilder()
                    .WithColor(Color.Orange)
                    .WithTitle("No milestone found")
                    .WithDescription($"Could not find a milestone with title \"{title}\".")
                    .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                    .WithCurrentTimestamp()
                    .Build());
            }
            else
            {
                milestone.DeletedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
                await interaction.FollowupAsync(embed: new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithTitle("Milestone has been deleted")
                    .WithDescription($"Deleted milestone with title \"{title}\".")
                    .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                    .WithCurrentTimestamp()
                    .Build());
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Creating milestone failed: {ex.Message}", ex);
            await interaction.FollowupAsync(embed: new EmbedBuilder()
                .WithColor(Color.Orange)
                .WithTitle("An error occurred")
                .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                .WithCurrentTimestamp()
                .Build());
        }
    }

    public async Task ReachMilestoneAsync(SocketInteraction interaction, string title, DateTimeOffset date)
    {
        await interaction.DeferAsync();
        try
        {
            var milestone = await dbContext.Milestones
                .OrderBy(m => m.CreatedAt)
                .Where(m => m.User.DiscordId == interaction.User.Id)
                .Where(m => m.Title == title)
                .Where(m => m.ReachedAt == null)
                .FirstOrDefaultAsync();
            if (milestone == null)
            {
                await interaction.FollowupAsync(embed: new EmbedBuilder()
                    .WithColor(Color.Orange)
                    .WithTitle("No milestone found")
                    .WithDescription($"Could not find a milestone with title \"{title}\".")
                    .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                    .WithCurrentTimestamp()
                    .Build());
            }
            else
            {
                milestone.ReachedAt = date;
                await dbContext.SaveChangesAsync();
                await interaction.FollowupAsync(embed: new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithTitle("Milestone has been reached ")
                    .WithDescription($"🎉 Congratulations! 🎉 You've reached your milestone for \"{title}\"! 🎯 Well done! 🙌")
                    .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                    .WithCurrentTimestamp()
                    .Build());
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Creating milestone failed: {ex.Message}", ex);
            await interaction.FollowupAsync(embed: new EmbedBuilder()
                .WithColor(Color.Orange)
                .WithTitle("An error occurred")
                .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                .WithCurrentTimestamp()
                .Build());
        }
    }
}