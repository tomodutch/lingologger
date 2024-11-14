namespace LingoLogger.Data.Models;

public class Log
{
    public required string Title { get; set; }
    public LogType LogType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTimeOffset? DeletedAt { get; set; }
    public required string Medium { get; set; }
    public int AmountOfSeconds { get; set; }
    public double Coefficient { get; set; }
    public required string Source { get; set; }
    public string? SourceEventId { get; set; }
    public int? CharactersRead { get; set; }
}
