using AutoFixture.NUnit3;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using LingoLogger.Data.Access;
using LingoLogger.Discord.Bot.InteractionHandlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Testcontainers.PostgreSql;

namespace test.discord.bot;

public class TestSocketInteractionContext : SocketInteractionContext
{
    public TestSocketInteractionContext(DiscordSocketClient client, SocketInteraction interaction)
        : base(client, interaction)
    {
    }
}

public class Tests
{
    private PostgreSqlContainer _container;
    private LingoLoggerDbContext _dbContext;
    [SetUp]
    public async Task SetupAsync()
    {
        _container = new PostgreSqlBuilder()
          .WithImage("timescale/timescaledb-ha:pg16")
          .Build();
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<LingoLoggerDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;
        _dbContext = new LingoLoggerDbContext(options);
        await _dbContext.Database.MigrateAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _dbContext.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }

    [Test, AutoData]
    public async Task TestCreateLog()
    {
        var userId = ulong.MinValue;
        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns(userId);
        user.Setup(u => u.GetAvatarUrl(It.IsAny<ImageFormat>(), It.IsAny<ushort>())).Returns("http://example.com");
        var interaction = new Mock<IDiscordInteraction>();
        interaction.Setup(i => i.User).Returns(user.Object);
        var service = new LogService(new NullLogger<LogService>(), _dbContext);
        await service.LogReadAsync(
            interaction.Object,
            "Read",
            "100m",
            "test-title",
            characters: 100,
            notes: "test"
        );

        var logs = await _dbContext.Logs.Where(l => l.User.DiscordId == userId).ToListAsync();
        Assert.That(logs, Is.Not.Empty);
    }
}