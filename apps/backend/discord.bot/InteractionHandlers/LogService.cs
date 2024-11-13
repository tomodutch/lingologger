using System.Text;
using Discord;
using LingoLogger.Data.Access;
using LingoLogger.Data.Models;
using LingoLogger.Discord.Bot.InteractionParameters;
using LingoLogger.Discord.Bot.Services;
using LingoLogger.Discord.Bot.Validators;
using LingoLogger.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot.InteractionHandlers;

public class LogService(ILogger<LogService> logger, LingoLoggerDbContext dbContext, LogReadParametersValidator logReadParamsValidator, UserService userService, TimeParser timeParser)
{
    public async Task LogReadAsync(IDiscordInteraction interaction, LogReadParameters param)
    {
        var validationResult = logReadParamsValidator.Validate(param);

        if (!validationResult.IsValid) {
            await interaction.RespondAsync(embed: new EmbedBuilder()
                .WithThumbnailUrl(interaction.User.GetAvatarUrl())
                .WithTitle("Invalid input")
                .WithDescription("Could not create log")
                .WithFields(validationResult.Errors.Select(e => new EmbedFieldBuilder().WithName(e.PropertyName).WithValue(e.ErrorMessage)))
                .WithCurrentTimestamp()
                .Build());
            return;
        }

        await interaction.DeferAsync();
        using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            logger.LogInformation($"Incoming readable {DateTimeOffset.UtcNow}");
            var seconds = timeParser.ParseTimeToSeconds(param.Time);
            var user = await userService.GetOrCreateUserAsync(interaction.User.Id);
            var dbLog = new ReadableLog()
            {
                Title = param.Title,
                Medium = param.Medium,
                AmountOfSeconds = seconds,
                Source = "Discord",
            };
            SetCreatedAtIfBacklog(param.Date, dbLog);
            if (param.Characters.HasValue)
            {
                dbLog.CharactersRead = param.Characters;
                dbLog.Coefficient = param.Characters.Value / (seconds / 3600.0);
            }
            user.Logs.Add(dbLog);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            var description = $"Title: {param.Title}\nTime: {param.Time}\nCharacters: {(param.Characters.HasValue ? param.Characters.Value.ToString() : "N/A")}\nNotes: {param.Notes ?? "No notes provided."}";
            await interaction.FollowupAsync("logged", embed: BuildCreatedLogEmbed(interaction, description));
            logger.LogInformation($"Send response {DateTimeOffset.UtcNow}");
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed logging readable: {ex.Message}", ex);
            await transaction.RollbackAsync();
            await interaction.FollowupAsync("An error occurred while logging. Try again later");
        }
    }


    public async Task LogAudibleAsync(IDiscordInteraction interaction, string medium, string time, string title, string? notes, string? createdAtString = null)
    {
        using var transaction = await dbContext.Database.BeginTransactionAsync();
        await interaction.DeferAsync();
        try
        {
            var seconds = timeParser.ParseTimeToSeconds(time);
            var user = await userService.GetOrCreateUserAsync(interaction.User.Id);
            var dbLog = new AudibleLog()
            {
                Title = title,
                Medium = medium,
                AmountOfSeconds = seconds,
                Source = "Discord",
            };
            SetCreatedAtIfBacklog(createdAtString, dbLog);
            user.Logs.Add(dbLog);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            var description = "Listening logged";
            await interaction.FollowupAsync(embed: BuildCreatedLogEmbed(interaction, description));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError($"Failed logging audible: {ex.Message}", ex);
            await interaction.FollowupAsync("An error occurred while logging. Try again later");
        }
    }

    public async Task LogWatchableAsync(IDiscordInteraction interaction, string medium, string time, string title, string? notes, string? createdAtString = null)
    {
        using var transaction = await dbContext.Database.BeginTransactionAsync();
        await interaction.DeferAsync();
        try
        {
            var seconds = timeParser.ParseTimeToSeconds(time);
            var user = await userService.GetOrCreateUserAsync(interaction.User.Id);
            var dbLog = new WatchableLog()
            {
                Title = title,
                Medium = medium,
                AmountOfSeconds = seconds,
                Source = "Discord",
            };
            SetCreatedAtIfBacklog(createdAtString, dbLog);
            user.Logs.Add(dbLog);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var description = "Watchable logged";
            await interaction.FollowupAsync(embed: BuildCreatedLogEmbed(interaction, description));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError($"Failed logging watchable: {ex.Message}", ex);
            await interaction.FollowupAsync("An error occurred while logging. Try again later");
        }
    }

    public async Task LogEpisodicAsync(IDiscordInteraction interaction, string medium, int amountOfEpisodes, string episodeLength, string title, string? notes, string? createdAtString = null)
    {
        using var transaction = await dbContext.Database.BeginTransactionAsync();
        await interaction.DeferAsync();
        try
        {
            var episodeLengthInSeconds = timeParser.ParseTimeToSeconds(episodeLength);
            var seconds = amountOfEpisodes * episodeLengthInSeconds;
            var user = await userService.GetOrCreateUserAsync(interaction.User.Id);
            var dbLog = new EpisodicLog()
            {
                Title = title,
                Medium = medium,
                AmountOfSeconds = seconds,
                EpisodeLengthInSeconds = episodeLengthInSeconds,
                Episodes = amountOfEpisodes,
                Source = "Discord",
            };
            SetCreatedAtIfBacklog(createdAtString, dbLog);
            user.Logs.Add(dbLog);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var description = "Watchable logged";
            await interaction.FollowupAsync(embed: BuildCreatedLogEmbed(interaction, description));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError($"Failed logging episodic: {ex.Message}", ex);
            await interaction.FollowupAsync("An error occurred while logging. Try again later");
        }
    }

    public async Task UndoMostRecentLogAsync(IDiscordInteraction interaction)
    {
        await interaction.DeferAsync();
        try
        {
            var log = await dbContext.Logs
                .Where(l => l.User.DiscordId == interaction.User.Id)
                .OrderByDescending(l => l.CreatedAt)
                .FirstOrDefaultAsync();

            if (log == null)
            {
                await interaction.FollowupAsync("No logs found for this user");
            }
            else
            {
                log.DeletedAt = DateTimeOffset.UtcNow;
                await dbContext.SaveChangesAsync();
                var time = timeParser.SecondsToTimeFormat(log.AmountOfSeconds);
                var embedBuilder = new EmbedBuilder();
                embedBuilder = embedBuilder
                    .WithTitle("Log undone")
                    .WithColor(Color.Blue)
                    .WithDescription($"{log.CreatedAt}: {time} {log.Title}")
                    .WithCurrentTimestamp();

                await interaction.FollowupAsync("Log undone", embed: embedBuilder.Build());
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Undo: {ex.Message}", ex);
            await interaction.FollowupAsync("Could not undo most recent log. Please try again later.");
        }
    }

    public async Task GetLogsAsync(IDiscordInteraction interaction)
    {
        await interaction.DeferAsync();
        try
        {
            var userId = interaction.User.Id;
            var logs = await dbContext.Logs
                .Where(l => l.User.DiscordId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .Take(25)
                .ToListAsync();

            var embedBuilder = new EmbedBuilder();
            if (logs == null || logs.Any() == false)
            {
                embedBuilder = embedBuilder
                    .WithTitle("Logs")
                    .WithColor(Color.Orange)
                    .WithDescription("No logs found for user");
            }
            else
            {
                embedBuilder = embedBuilder.WithTitle("Logs").WithColor(Color.Blue);
                var sb = new StringBuilder();
                foreach (var log in logs)
                {
                    var time = timeParser.SecondsToTimeFormat(log.AmountOfSeconds);
                    sb.Append($"- **{log.CreatedAt.ToString("yyyy-MM-dd")}**: {time} {log.Title}  \n");
                }

                embedBuilder.WithDescription(sb.ToString());
            }
            await interaction.FollowupAsync("/logs", embed: embedBuilder.Build());
        }
        catch (Exception ex)
        {
            logger.LogError($"/logs: {ex.Message}", ex);
            await interaction.FollowupAsync("An error occurred while fetching the logs. Please try again later.");
        }
    }

    private static Embed BuildCreatedLogEmbed(IDiscordInteraction interaction, string description)
    {
        var embed = new EmbedBuilder()
            .WithTitle("Created log")
            .WithDescription(description)
            .WithColor(Color.Blue)
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithThumbnailUrl(interaction.User.GetAvatarUrl())
            .WithImageUrl("https://i.giphy.com/media/v1.Y2lkPTc5MGI3NjExb3VuNHJsdmloOWNtaHc5NGR0ZjcxeXcxeXZucjgzdHdranRhdzE4cSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/WjPmpgwiaxwHgohCfD/giphy.gif")
            .WithFooter("Thank you for logging your reading activity!");
        return embed.Build();
    }

    private void SetCreatedAtIfBacklog(string? createdAtString, Log dbLog)
    {
        if (createdAtString == null)
        {
            return;
        }

        var createdAt = timeParser.ParseDate(createdAtString);
        if (createdAt.HasValue)
        {
            dbLog.CreatedAt = createdAt.Value;
        }
    }
}