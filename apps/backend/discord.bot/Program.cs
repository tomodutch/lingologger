using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using LingoLogger.Data.Access;
using Microsoft.EntityFrameworkCore;
using LingoLogger.Data.Models.Stores;
using LingoLogger.Data.Access.Stores;
using LingoLogger.Web.Models;

namespace LingoLogger.Discord.Bot;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args);
        await host.RunConsoleAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory)
                      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                      .AddUserSecrets<BotHostedService>(optional: true)
                      .AddEnvironmentVariables();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
                // Optionally add other providers, such as AddEventLog or AddFile
            })
            .ConfigureServices(ConfigureServices);

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        var connectionString = context.Configuration.GetConnectionString("DbConnection");

        services.AddDbContext<LingoLoggerDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddSingleton<DiscordSocketClient>()
                .AddTransient<ILogStore, LogStore>()
                .AddTransient<TimeParser>()
                .AddSingleton(x =>
                {
                    var client = x.GetRequiredService<DiscordSocketClient>();
                    return new InteractionService(client.Rest);
                })
                .AddSingleton(x => new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Debug,
                    GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages,
                    AlwaysDownloadUsers = true
                })
                .AddHostedService<BotHostedService>();

        services.AddLogging();
    }
}