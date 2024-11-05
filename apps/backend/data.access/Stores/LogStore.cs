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

    public async Task<IEnumerable<ApiLog>> GetLogsAsync(ulong discordId)
    {
        var logs = await _dbContext.Logs
            .Where(l => l.User != null && l.User.DiscordId == discordId)
            .OrderByDescending(l => l.CreatedAt)
            .Take(25)
            .ToListAsync();

        if (logs == null)
        {
            return [];
        }

        var apiLogs = logs.Select(MapTo);
        return apiLogs;
    }

    private ApiLog MapTo(Log log)
    {
        return log switch
        {
            ReadableLog l => new ApiReadableLog()
            {
                Title = log.Title,
                CharactersRead = l.CharactersRead,
                Time = _timeParser.SecondsToTimeFormat(l.AmountOfSeconds),
                Source = l.Source,
                Medium = l.Medium,
                CreatedAt = l.CreatedAt,
            },
            WatchableLog l => new ApiWatchableLog()
            {
                Title = log.Title,
                Time = _timeParser.SecondsToTimeFormat(l.AmountOfSeconds),
                Source = l.Source,
                Medium = l.Medium,
                CreatedAt = l.CreatedAt,
            },
            AudibleLog l => new ApiAudibleLog()
            {
                Title = log.Title,
                Time = _timeParser.SecondsToTimeFormat(l.AmountOfSeconds),
                Source = l.Source,
                Medium = l.Medium,
                CreatedAt = l.CreatedAt,
            },
            EpisodicLog l => new ApiEpisodicLog()
            {
                Title = log.Title,
                Time = _timeParser.SecondsToTimeFormat(l.AmountOfSeconds),
                Source = l.Source,
                Medium = l.Medium,
                AmountOfEpisodes = l.Episodes,
                EpisodeLength = "",
                CreatedAt = l.CreatedAt,
            },
            _ => throw new NotImplementedException(),
        };
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

    public async Task<ApiLog?> UndoMostRecentLogAsync(ulong discordId)
    {
        var log = await _dbContext.Logs
            .Where(l => l.User.DiscordId == discordId)
            .OrderByDescending(l => l.CreatedAt)
            .FirstOrDefaultAsync();
        if (log == null)
        {
            return null;
        }

        log.DeletedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();

        return MapTo(log);
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

    public async Task<ApiProfile> GetProfileAsync(ulong discordId)
    {
        var today = DateTimeOffset.UtcNow;
        var logs = await _dbContext.Logs.Where(l => l.User.DiscordId == discordId)
            .Where(l => l.CreatedAt.UtcDateTime.Year == today.Year && l.CreatedAt.UtcDateTime.DayOfYear == today.DayOfYear)
            .ToListAsync();

        var profile = new ApiProfile()
        {
            ReadTimeFormatted = "0s",
            ReadTimeInSeconds = 0,
            ListenTimeFormatted = "0s",
            ListenTimeInSeconds = 0,
            WatchTimeFormatted = "0s",
            WatchTimeInSeconds = 0,
            EpisodesWatched = 0
        };

        foreach (var log in logs)
        {
            switch (log)
            {
                case ReadableLog readableLog:
                    profile.ReadTimeInSeconds += readableLog.AmountOfSeconds;
                    break;
                case AudibleLog audibleLog:
                    profile.ListenTimeInSeconds += audibleLog.AmountOfSeconds;
                    break;
                case WatchableLog watchableLog:
                    profile.WatchTimeInSeconds += watchableLog.AmountOfSeconds;
                    break;
                case EpisodicLog episodicLog:
                    profile.WatchTimeInSeconds += episodicLog.AmountOfSeconds;
                    profile.EpisodesWatched += episodicLog.Episodes;
                    break;
                default:
                    _logger.LogWarning($"{log.GetType()}: not supported in profile.");
                    break;
            }
        }

        profile.ReadTimeFormatted = _timeParser.SecondsToTimeFormat(profile.ReadTimeInSeconds);
        profile.ListenTimeFormatted = _timeParser.SecondsToTimeFormat(profile.ListenTimeInSeconds);
        profile.WatchTimeFormatted = _timeParser.SecondsToTimeFormat(profile.WatchTimeInSeconds);

        return profile;
    }
}