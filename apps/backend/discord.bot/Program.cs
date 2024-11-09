﻿using Discord;
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
using OpenTelemetry;
using Grafana.OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using FluentValidation;
using LingoLogger.Discord.Bot.Validators;

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
            var chartApiUri = context.Configuration.GetValue<Uri>("ChartApiUri");
            client.BaseAddress = chartApiUri;
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddSingleton<DiscordSocketClient>()
                .AddTransient<SocketInteractionContext>()
                .AddTransient<TimeParser>()
                .AddTransient<LogService>()
                .AddTransient<UserService>()
                .AddTransient<ProfileService>()
                .AddTransient<GoalService>()
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
                .AddValidatorsFromAssemblyContaining<LogReadParametersValidator>()
                .AddHostedService<BotHostedService>();
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .UseGrafana()
            .Build();
        var meterProviderBuilder = Sdk.CreateMeterProviderBuilder().UseGrafana();
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            meterProviderBuilder = meterProviderBuilder.AddConsoleExporter();
        }
        using var meterProvider = meterProviderBuilder.Build();
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddOpenTelemetry(logging =>
            {
                logging.UseGrafana();
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    logging.AddConsoleExporter();
                }
            });
        });
        // services.AddLogging();
    }
}