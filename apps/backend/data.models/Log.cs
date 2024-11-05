namespace LingoLogger.Data.Models;

public abstract class Log
{
    public required string Title { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTimeOffset? DeletedAt { get; set; }
    public required string Medium { get; set; }
    public int AmountOfSeconds { get; set; }
    public double Coefficient { get; set; }
    public required string Source { get; set; }
}

public class ReadableLog : Log
{
    public int? CharactersRead { get; set; }
}

public class AudibleLog : Log
{
}

public class WatchableLog : Log
{
}

public class EpisodicLog : Log
{
    public int? Episodes { get; set; }
    public int? EpisodeLengthInSeconds { get; set; }
}