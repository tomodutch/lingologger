namespace LingoLogger.Discord.Bot.InteractionParameters;

public class LogReadParameters
{
    public required string Medium { get; set; }
    public required string Time { get; set; }
    public required string Title { get; set; }
    public int? Characters { get; set; }
    public string? Notes { get; set; }
    public string? Date { get; set; }
}