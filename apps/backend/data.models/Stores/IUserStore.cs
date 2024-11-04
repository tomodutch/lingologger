namespace LingoLogger.Data.Models.Stores;

public interface IUserStore
{
    public Task<User> GetOrCreateByDiscordIdAsync(ulong discordId);
}