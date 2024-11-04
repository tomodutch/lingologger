using LingoLogger.Data.Models;
using LingoLogger.Data.Models.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Data.Access.Stores;

public class UserStore : IUserStore
{
    private readonly LingoLoggerDbContext _dbContext;
    private readonly ILogger<UserStore> _logger;

    public UserStore(ILogger<UserStore> logger, LingoLoggerDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<User> GetOrCreateByDiscordIdAsync(ulong discordId)
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