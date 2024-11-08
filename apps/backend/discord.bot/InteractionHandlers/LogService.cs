using System.Globalization;
using System.Text;
using Discord;
using LingoLogger.Data.Access;
using LingoLogger.Data.Models;
using LingoLogger.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot.InteractionHandlers;

public class LogService(ILogger<LogService> logger, LingoLoggerDbContext dbContext)
{
    private readonly ILogger<LogService> _logger = logger;
    private readonly LingoLoggerDbContext _dbContext = dbContext;
    private readonly TimeParser _timeParser = new();

    public async Task LogReadAsync(IDiscordInteraction interaction, string medium, string time, string title, int? characters, string? notes, string? createdAtString = null)
    {
        await interaction.DeferAsync();
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            _logger.LogInformation($"Incoming readable {DateTimeOffset.UtcNow}");
            var seconds = _timeParser.ParseTimeToSeconds(time);
            var user = await GetOrCreateUserAsync(interaction.User.Id);
            var dbLog = new ReadableLog()
            {
                Title = title,
                Medium = medium,
                AmountOfSeconds = seconds,
                Source = "Discord",
            };
            SetCreatedAtIfBacklog(createdAtString, dbLog);
            if (characters.HasValue)
            {
                dbLog.CharactersRead = characters;
                dbLog.Coefficient = characters.Value / (seconds / 3600.0);
            }
            user.Logs.Add(dbLog);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            var description = $"Title: {title}\nTime: {time}\nCharacters: {(characters.HasValue ? characters.Value.ToString() : "N/A")}\nNotes: {notes ?? "No notes provided."}";
            await interaction.FollowupAsync("logged", embed: BuildCreatedLogEmbed(interaction, description));
            _logger.LogInformation($"Send response {DateTimeOffset.UtcNow}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed logging readable: {ex.Message}", ex);
            await transaction.RollbackAsync();
            await interaction.FollowupAsync("An error occurred while logging. Try again later");
        }
    }


    public async Task LogAudibleAsync(IDiscordInteraction interaction, string medium, string time, string title, string? notes, string? createdAtString = null)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        await interaction.DeferAsync();
        try
        {
            var seconds = _timeParser.ParseTimeToSeconds(time);
            var user = await GetOrCreateUserAsync(interaction.User.Id);
            var dbLog = new AudibleLog()
            {
                Title = title,
                Medium = medium,
                AmountOfSeconds = seconds,
                Source = "Discord",
            };
            SetCreatedAtIfBacklog(createdAtString, dbLog);
            user.Logs.Add(dbLog);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            var description = "Listening logged";
            await interaction.FollowupAsync(embed: BuildCreatedLogEmbed(interaction, description));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError($"Failed logging audible: {ex.Message}", ex);
            await interaction.FollowupAsync("An error occurred while logging. Try again later");
        }
    }

    public async Task LogWatchableAsync(IDiscordInteraction interaction, string medium, string time, string title, string? notes, string? createdAtString = null)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        await interaction.DeferAsync();
        try
        {
            var seconds = _timeParser.ParseTimeToSeconds(time);
            var user = await GetOrCreateUserAsync(interaction.User.Id);
            var dbLog = new WatchableLog()
            {
                Title = title,
                Medium = medium,
                AmountOfSeconds = seconds,
                Source = "Discord",
            };
            SetCreatedAtIfBacklog(createdAtString, dbLog);
            user.Logs.Add(dbLog);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var description = "Watchable logged";
            await interaction.FollowupAsync(embed: BuildCreatedLogEmbed(interaction, description));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError($"Failed logging watchable: {ex.Message}", ex);
            await interaction.FollowupAsync("An error occurred while logging. Try again later");
        }
    }

    public async Task LogEpisodicAsync(IDiscordInteraction interaction, string medium, int amountOfEpisodes, string episodeLength, string title, string? notes, string? createdAtString = null)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        await interaction.DeferAsync();
        try
        {
            var episodeLengthInSeconds = _timeParser.ParseTimeToSeconds(episodeLength);
            var seconds = amountOfEpisodes * episodeLengthInSeconds;
            var user = await GetOrCreateUserAsync(interaction.User.Id);
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
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var description = "Watchable logged";
            await interaction.FollowupAsync(embed: BuildCreatedLogEmbed(interaction, description));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError($"Failed logging episodic: {ex.Message}", ex);
            await interaction.FollowupAsync("An error occurred while logging. Try again later");
        }
    }

    private async Task<User> GetOrCreateUserAsync(ulong discordId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.DiscordId == discordId);
        if (user == null)
        {
            user = new User()
            {
                DiscordId = discordId,
            };
            await _dbContext.AddAsync(user);
        }

        return user;
    }

    public async Task UndoMostRecentLogAsync(IDiscordInteraction interaction)
    {
        await interaction.DeferAsync();
        try
        {
            var log = await _dbContext.Logs
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
                await _dbContext.SaveChangesAsync();
                var time = _timeParser.SecondsToTimeFormat(log.AmountOfSeconds);
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
            _logger.LogError($"Undo: {ex.Message}", ex);
            await interaction.FollowupAsync("Could not undo most recent log. Please try again later.");
        }
    }

    public async Task GetLogsAsync(IDiscordInteraction interaction)
    {
        await interaction.DeferAsync();
        try
        {
            var userId = interaction.User.Id;
            // var logs = await _logStore.GetLogsAsync(userId);
            var logs = _dbContext.Logs
                .Where(l => l.User.DiscordId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .Take(25);

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
                    var time = _timeParser.SecondsToTimeFormat(log.AmountOfSeconds);
                    sb.Append($"{log.CreatedAt}: {time} {log.Medium} {log.Title}  \n");
                }

                embedBuilder.WithDescription(sb.ToString());
            }
            await interaction.FollowupAsync("/logs", embed: embedBuilder.Build());
        }
        catch (Exception ex)
        {
            _logger.LogError($"/logs: {ex.Message}", ex);
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
            .WithFooter("Thank you for logging your reading activity!");
        return embed.Build();
    }

    private void SetCreatedAtIfBacklog(string? createdAtString, Log dbLog)
    {
        if (createdAtString == null)
        {
            return;
        }

        var createdAt = _timeParser.ParseBacklogDate(createdAtString);
        if (createdAt.HasValue)
        {
            dbLog.CreatedAt = createdAt.Value;
        }
    }
}