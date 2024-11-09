
using Discord;
using LingoLogger.Data.Access;
using LingoLogger.Data.Models;
using LingoLogger.Discord.Bot.InteractionParameters;
using LingoLogger.Discord.Bot.Services;
using LingoLogger.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot.InteractionHandlers;

public class GoalService(ILogger<GoalService> logger, LingoLoggerDbContext dbContext, TimeParser timeParser, UserService userService)
{
    private readonly ILogger<GoalService> _logger = logger;
    private readonly LingoLoggerDbContext _dbContext = dbContext;
    private readonly TimeParser _timeParser = timeParser;
    private readonly UserService _userService = userService;

    public async Task GetGoalsAsync(IDiscordInteraction interaction)
    {
        await interaction.DeferAsync();
        try
        {
            var goals = await _dbContext.Goals
                .Where(g => g.User.DiscordId == interaction.User.Id)
                .OrderByDescending(g => g.EndsAt)
                .Select(g => new
                {
                    Goal = g,
                    SpentTime = _dbContext.Logs
                        .Where(l => l.UserId == g.UserId && l.CreatedAt.Date >= g.CreatedAt.Date && l.CreatedAt.Date <= g.EndsAt!.Value.Date)
                        .Sum(l => l.AmountOfSeconds)
                })
                .ToListAsync();
            var embedBuilder = new EmbedBuilder()
                .WithTitle($"{interaction.User.GlobalName}'s goals")
                .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                .WithImageUrl("https://media.giphy.com/media/kKtAJrJUQnuikFZr3c/giphy.gif?cid=790b7611n4cmq8azlqxjd0m730di68l7jp7o6ne3hot5nfyx&ep=v1_gifs_search&rid=giphy.gif&ct=g")
                .WithCurrentTimestamp();
            var description = string.Join("\n", goals.Select(g =>
            {
                var time = _timeParser.SecondsToTimeFormat(g.SpentTime);
                var targetTime = _timeParser.SecondsToTimeFormat(g.Goal.TargetTimeInSeconds);
                var progress = Math.Round((double)g.SpentTime / g.Goal.TargetTimeInSeconds * 100, 1);
                return $"- {time}/{targetTime} ({progress}%)";
            }));

            await interaction.FollowupAsync(embed: embedBuilder.WithDescription(description).Build());
        }
        catch (Exception ex)
        {
            _logger.LogError("get goals failed", ex);
            var embedBuilder = new EmbedBuilder()
                .WithColor(Color.Orange)
                .WithTitle($"{interaction.User.GlobalName}'s goals could not be fetched")
                .WithDescription("Error fetching goals. Please try again later")
                .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                .WithImageUrl("https://media.giphy.com/media/v1.Y2lkPTc5MGI3NjExZzNtcWxlc2w4d3ozdXRjNjM3M3IwdjV3YW1qMjJ1anBqOXlkenBpciZlcD12MV9naWZzX3NlYXJjaCZjdD1n/uDIfs260heCkUagE76/giphy.gif")
                .WithCurrentTimestamp();
            await interaction.FollowupAsync(embed: embedBuilder.Build());
        }
    }

    public async Task CreateGoalAsync(IDiscordInteraction interaction, CreateGoalParameters p)
    {
        await interaction.DeferAsync();
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var user = await _userService.GetOrCreateUserAsync(interaction.User.Id);
            var target = _timeParser.ParseTimeToSeconds(p.TargetTime);
            var endsAt = _timeParser.ParseDate(p.Until);

            var goal = new Goal()
            {
                UserId = user.Id,
                TargetTimeInSeconds = target,
                EndsAt = endsAt
            };
            user.Goals.Add(goal);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            await interaction.FollowupAsync(embed: new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithTitle("Created goal")
                .WithDescription("TODO: goal")
                .WithImageUrl("https://media.giphy.com/media/v1.Y2lkPTc5MGI3NjExZzNtcWxlc2w4d3ozdXRjNjM3M3IwdjV3YW1qMjJ1anBqOXlkenBpciZlcD12MV9naWZzX3NlYXJjaCZjdD1n/CkTRTXDIFhix8Vp5Oz/giphy.gif")
                .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                .WithCurrentTimestamp()
                .Build());
        }
        catch (Exception ex)
        {
            _logger.LogError("Creating goal failed", ex);
            await interaction.FollowupAsync(embed: new EmbedBuilder()
                .WithColor(Color.Orange)
                .WithTitle("Goal could not be created")
                .WithImageUrl("https://media.giphy.com/media/v1.Y2lkPTc5MGI3NjExZWVtNzYyM2c0MTVjaWo3c3ViZmxzYW5uemppMHB3aW51YTc1OThiaiZlcD12MV9naWZzX3NlYXJjaCZjdD1n/nM9Fzo0gFnCyQKv28u/giphy.gif")
                .WithDescription("Error creating goal. Please try again later")
                .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                .WithCurrentTimestamp()
                .Build());
        }

    }
}