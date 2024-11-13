namespace LingoLogger.Data.Models;

public class Milestone
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ReachedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}