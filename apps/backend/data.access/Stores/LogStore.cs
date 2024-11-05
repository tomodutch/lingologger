using LingoLogger.Data.Models;
using LingoLogger.Data.Models.Stores;
using LingoLogger.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Data.Access.Stores;

public class LogStore : ILogStore
{
    private readonly ILogger<LogStore> _logger;
    private readonly LingoLoggerDbContext _dbContext;
    private readonly TimeParser _timeParser;

    public LogStore(ILogger<LogStore> logger, LingoLoggerDbContext dbContext, TimeParser timeParser)
    {
        _logger = logger;
        _dbContext = dbContext;
        _timeParser = timeParser;
    }

    public async Task SaveLogAsync(ApiReadableLog log, SaveLogOptions options)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var seconds = _timeParser.ParseTimeToSeconds(log.Time);
            var user = await GetOrCreateUserAsync(options.UserId, options.DiscordId);
            var dbLog = new ReadableLog()
            {
                Title = log.Title,
                Medium = log.Medium,
                AmountOfSeconds = seconds,
                Source = log.Source,
            };
            if (log.CharactersRead.HasValue)
            {
                dbLog.CharactersRead = log.CharactersRead;
                dbLog.Coefficient = log.CharactersRead.Value / (seconds / 3600.0);
            }
            user.Logs.Add(dbLog);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError($"Failed saving reading log: {ex.Message}", ex);
        }
    }

    public async Task SaveLogAsync(ApiAudibleLog log, SaveLogOptions options)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var seconds = _timeParser.ParseTimeToSeconds(log.Time);
            var user = await GetOrCreateUserAsync(options.UserId, options.DiscordId);
            var dbLog = new AudibleLog()
            {
                Title = log.Title,
                Medium = log.Medium,
                AmountOfSeconds = seconds,
                Source = log.Source,
            };
            user.Logs.Add(dbLog);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError($"Failed saving reading log: {ex.Message}", ex);
        }
    }

    public async Task SaveLogAsync(ApiWatchableLog log, SaveLogOptions options)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var seconds = _timeParser.ParseTimeToSeconds(log.Time);
            var user = await GetOrCreateUserAsync(options.UserId, options.DiscordId);
            var dbLog = new WatchableLog()
            {
                Title = log.Title,
                Medium = log.Medium,
                AmountOfSeconds = seconds,
                Source = log.Source,
            };

            user.Logs.Add(dbLog);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError($"Failed saving reading log: {ex.Message}", ex);
        }
    }

    public async Task SaveLogAsync(ApiEpisodicLog log, SaveLogOptions options)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var episodes = log.AmountOfEpisodes;
            var episodeLengthInSeconds = _timeParser.ParseTimeToSeconds(log.EpisodeLength);
            var seconds = episodes * episodeLengthInSeconds;
            var user = await GetOrCreateUserAsync(options.UserId, options.DiscordId);
            var dbLog = new EpisodicLog()
            {
                Title = log.Title,
                Medium = log.Medium,
                AmountOfSeconds = seconds,
                EpisodeLengthInSeconds = seconds,
                Episodes = episodes,
                Source = log.Source,
            };

            user.Logs.Add(dbLog);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError($"Failed saving reading log: {ex.Message}", ex);
        }
    }

    private async Task<User> GetOrCreateUserAsync(Guid? id, ulong? discordId)
    {
        User? user = null;
        if (id.HasValue)
        {
            user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id.Value);
        }
        else if (discordId.HasValue)
        {
            user = await GetOrCreateUserAsync(discordId.Value);
        }

        if (user == null)
        {
            throw new Exception("User not found");
        }

        return user;
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
}