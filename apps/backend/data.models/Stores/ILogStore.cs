using LingoLogger.Web.Models;

namespace LingoLogger.Data.Models.Stores;

public interface ILogStore
{
    public Task<IEnumerable<ApiLog>> GetLogsAsync(ulong discordId);
    public Task<ApiLog?> UndoMostRecentLogAsync(ulong discordId);
    public Task SaveLogAsync(ApiReadableLog log, SaveLogOptions options);
    public Task SaveLogAsync(ApiAudibleLog log, SaveLogOptions options);
    public Task SaveLogAsync(ApiWatchableLog log, SaveLogOptions options);
    public Task SaveLogAsync(ApiEpisodicLog log, SaveLogOptions options);
}


public enum SaveLogSource
{
    Discord,
    WebApp
}

public class SaveLogOptions
{
    public SaveLogSource Source { get; set; }
    public ulong? DiscordId { get; set; }
    public Guid? UserId { get; set; }
}