
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
                .Select(g => new
                {
                    Goal = g,
                    SpentTime = _dbContext.Logs
                        .Where(l => l.UserId == g.UserId && l.CreatedAt >= g.CreatedAt && l.CreatedAt <= g.EndsAt)
                        .Sum(l => l.AmountOfSeconds)
                })
                .ToListAsync();
            var embedBuilder = new EmbedBuilder()
                .WithTitle($"{interaction.User.GlobalName}'s goals")
                .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                .WithCurrentTimestamp();
            var description = string.Join(",", goals.Select(g =>
            {
                var time = _timeParser.SecondsToTimeFormat(g.SpentTime);
                var targetTime = _timeParser.SecondsToTimeFormat(g.Goal.TargetTimeInSeconds);
                var progress = Math.Round((double)g.SpentTime / g.Goal.TargetTimeInSeconds * 100, 1);
                return $"{time}/{targetTime} ({progress}%)";
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
                .WithDescription("Error creating goal. Please try again later")
                .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                .WithCurrentTimestamp()
                .Build());
        }

    }
}