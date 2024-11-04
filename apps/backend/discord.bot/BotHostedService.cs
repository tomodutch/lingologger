using Microsoft.Extensions.Hosting;
using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot;

public class BotHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BotHostedService> _logger;

    public BotHostedService(DiscordSocketClient client,
                            InteractionService interactionService,
                            IConfiguration configuration,
                            IServiceProvider serviceProvider,
                            ILogger<BotHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _interactionService = interactionService;
        _client = client;
        _logger = logger;
        _client.Log += Log;
        _client.Ready += OnReady;
        _client.InteractionCreated += HandleInteraction;
    }

    private Task Log(LogMessage msg)
    {
        var msgString = msg.ToString();
        switch (msg.Severity)
        {
            case LogSeverity.Info:
                _logger.LogInformation(msgString);
                break;
            case LogSeverity.Debug:
                _logger.LogDebug(msgString);
                break;
            case LogSeverity.Verbose:
                _logger.LogDebug(msgString);
                break;
            case LogSeverity.Warning:
                _logger.LogWarning(msgString);
                break;
            case LogSeverity.Error:
                _logger.LogError(msgString);
                break;
            case LogSeverity.Critical:
                _logger.LogCritical(msgString);
                break;
            default:
                _logger.LogInformation(msgString);
                break;
        }
        return Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var token = _configuration["Discord:BotToken"];
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync().ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.LogoutAsync();
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(_client, interaction);
        try
        {
            await _interactionService.ExecuteCommandAsync(context, _serviceProvider).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling interaction: {ex.Message}", ex);
            if (interaction.Type == InteractionType.ApplicationCommand)
            {
                await interaction.RespondAsync("There was an error processing your command.");
            }
        }
    }

    private async Task OnReady()
    {
        ulong guildId = 1302521608271040554;
        await _client.Rest.DeleteAllGlobalCommandsAsync();
        // Register all commands from the InteractionService
        var modules = await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), services: _serviceProvider);
        _logger.LogInformation($"Registered modules count: {modules.Count()}");
        await _interactionService.RegisterCommandsToGuildAsync(guildId);
        _logger.LogInformation("Bot is ready!");
    }
}