using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using LingoLogger.Data.Access;
using Microsoft.EntityFrameworkCore;
using LingoLogger.Web.Models;
using LingoLogger.Discord.Bot.InteractionHandlers;
using LingoLogger.Discord.Bot.Services;

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

        services.AddHttpClient<ChartService>((services, client) =>
        {
            var baseUrl = "http://localhost:5000";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddSingleton<DiscordSocketClient>()
                .AddTransient<SocketInteractionContext>()
                .AddTransient<TimeParser>()
                .AddTransient<LogService>()
                .AddTransient<ProfileService>()
                .AddSingleton(x =>
                {
                    var client = x.GetRequiredService<DiscordSocketClient>();
                    return new InteractionService(client.Rest);
                })
                .AddSingleton(x => new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Debug,
                    GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages,
                    AlwaysDownloadUsers = false
                })
                .AddHostedService<BotHostedService>();

        services.AddLogging();
    }
}