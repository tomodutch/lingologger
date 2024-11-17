using LingoLogger.Data.Models;

namespace LingoLogger.Discord.Bot.InteractionParameters;

public class LogParameters
{
    public required string Time { get; set; }
    public required string Title { get; set; }
    public required LogType LogType { get; set; }
    public int? Characters { get; set; }
    public string? Notes { get; set; }
    public string? Date { get; set; }
}