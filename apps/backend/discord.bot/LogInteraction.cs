using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using LingoLogger.Data.Models;
using LingoLogger.Data.Models.Stores;
using LingoLogger.Web.Models;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot;

[Group("log", "log")]
public class LogInteraction : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<LogInteraction> _logger;
    private readonly ILogStore _logStore;

    public LogInteraction(ILogger<LogInteraction> logger, ILogStore logStore)
    {
        _logger = logger;
        _logStore = logStore;
    }


    // Subcommand for logging reading
    [SlashCommand("read", "Log a reading activity.")]
    public async Task LogReading(
        [Summary("medium", "Type of media read.")]
        [Choice("Book", "book")]
        [Choice("Visual Novel", "vn")]
        [Choice("Light Novel", "ln")]
        [Choice("News", "news")] string medium,
        [Summary("time", "Time spent watching in minutes.")] string time,
        [Summary("title", "Title of the book or material read.")] string title,
        [Summary("notes", "Additional notes about the reading.")] string? notes = null,
        [Summary("characters", "Total number of characters read.")] int? characters = null)
    {
        var log = new ApiReadableLog()
        {
            Medium = medium,
            Title = title,
            CharactersRead = characters,
            Time = time,
            Source = "Discord"
        };
        await _logStore.SaveLogAsync(log, new()
        {
            Source = SaveLogSource.Discord,
            DiscordId = Context.User.Id
        });
        var embed = new EmbedBuilder()
            .WithTitle("Created log")
            .WithDescription($"Title: {title}\nTime: {time}\nCharacters: {(characters.HasValue ? characters.Value.ToString() : "N/A")}\nNotes: {notes ?? "No notes provided."}")
            .WithColor(Color.Blue)
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithThumbnailUrl(Context.User.GetAvatarUrl())
            .WithFooter("Thank you for logging your reading activity!");
        await RespondAsync(embed: embed.Build());
    }

    // Subcommand for logging watching
    [SlashCommand("watched", "Log a watching activity.")]
    public async Task LogWatching(
        [Summary("media", "Type of media being watched (e.g., YouTube, anime).")] string media,
        [Summary("time", "Time spent watching in minutes.")] int time,
        [Summary("title", "Title of the video or show.")] string? title,
        [Summary("episodes", "Number of episodes watched.")] int episodes = 0,
        [Summary("episode_length", "Length of each episode in minutes.")] int episode_length = 0)
    {
        // Logic for logging watching activity
        await RespondAsync($"Logged watching: Media: {media}, Time: {time}m, Title: {title}, Episodes: {episodes}, Episode Length: {episode_length}m");
    }

    // Subcommand for logging listening
    [SlashCommand("listened", "Log a listening activity.")]
    public async Task LogListening(
        [Summary("media", "Type of media being listened to.")] string media,
        [Summary("time", "Time spent listening in minutes.")] int time,
        [Summary("title", "Title of the audio.")] string? title)
    {
        // Logic for logging listening activity
        await RespondAsync($"Logged listening: Media: {media}, Time: {time}m, Title: {title}");
    }
}
