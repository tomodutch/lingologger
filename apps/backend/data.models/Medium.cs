namespace LingoLogger.Data.Models;

public class Medium
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public LogType LogType { get; set; }
    public long? GuildId { get; set; }
    public ICollection<Log> Logs { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}