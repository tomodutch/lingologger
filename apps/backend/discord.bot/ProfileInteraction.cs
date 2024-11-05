using System.Text;
using Discord;
using Discord.Interactions;
using LingoLogger.Data.Models.Stores;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot;

public class ProfileInteraction : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<ProfileInteraction> _logger;
    private readonly ILogStore _logStore;

    public ProfileInteraction(ILogger<ProfileInteraction> logger, ILogStore logStore)
    {
        _logger = logger;
        _logStore = logStore;
    }

    [SlashCommand("profile", "show all logs")]
    public async Task Logs()
    {
        await DeferAsync();
        try
        {
            var userId = Context.User.Id;
            var profile = await _logStore.GetProfileAsync(userId);
            if (profile == null)
            {
                await FollowupAsync("No logs found.");
            }
            else
            {
                var embedBuilder = new EmbedBuilder();
                embedBuilder = embedBuilder
                    .WithColor(Color.Blue)
                    .WithThumbnailUrl(Context.User.GetAvatarUrl())
                    .WithTitle($"{Context.User.GlobalName}'s profile")
                    .WithDescription("Stats for today")
                    .AddField("Reading", profile.ReadTimeFormatted)
                    .AddField("Listening", profile.ListenTimeFormatted)
                    .AddField("Watching", profile.WatchTimeFormatted)
                    .AddField("Episodes watched", profile.EpisodesWatched)
                    .WithCurrentTimestamp();
                await FollowupAsync(embed: embedBuilder.Build());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"/profile: {ex.Message}", ex);
            await FollowupAsync("An error occurred while fetching the profile. Please try again later.");
        }
    }
}