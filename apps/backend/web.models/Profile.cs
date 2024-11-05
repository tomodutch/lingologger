namespace LingoLogger.Web.Models;

public class ApiProfile
{
    public int ReadTimeInSeconds { get; set; }
    public required string ReadTimeFormatted { get; set; }
    public int ListenTimeInSeconds { get; set; }
    public required string ListenTimeFormatted { get; set; }
    public int WatchTimeInSeconds { get; set; }
    public required string WatchTimeFormatted { get; set; }
    public int EpisodesWatched { get; set; }
}