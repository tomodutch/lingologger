using System.Text;
using Discord;
using Discord.Interactions;
using LingoLogger.Data.Models.Stores;
using LingoLogger.Discord.Bot.InteractionHandlers;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot;

public class ProfileInteraction : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<ProfileInteraction> _logger;
    private readonly ProfileService _service;

    public ProfileInteraction(ILogger<ProfileInteraction> logger, ProfileService service)
    {
        _logger = logger;
        _service = service;
    }

    [SlashCommand("profile", "show a profile")]
    public async Task Profile()
    {
        await _service.GetProfileAsync(Context.Interaction);
    }
}