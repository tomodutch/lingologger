namespace LingoLogger.Web.Models;

public class ApiLog
{
    public required string Title { get; set; }
    public required string Medium { get; set; }
    public required string Time { get; set; }
    public required string Source { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

}

public class ApiReadableLog : ApiLog
{
    public int? CharactersRead { get; set; }
}

public class ApiAudibleLog : ApiLog
{
}

public class ApiWatchableLog : ApiLog
{
}

public class ApiEpisodicLog : ApiLog
{
    public int AmountOfEpisodes { get; set; }
    public required string EpisodeLength { get; set; }
}
