using LingoLogger.Data.Access;
using LingoLogger.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot.Services;

public class UserService(ILogger<UserService> logger, LingoLoggerDbContext dbContext)
{
    private readonly ILogger<UserService> _logger = logger;
    private readonly LingoLoggerDbContext _dbContext = dbContext;
    public async Task<User> GetOrCreateUserAsync(ulong discordId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.DiscordId == discordId);
        if (user == null)
        {
            _logger.LogInformation($"Creating user with discord id {discordId}");
            user = new User()
            {
                DiscordId = discordId,
            };
            await _dbContext.AddAsync(user);
        }

        return user;
    }
}