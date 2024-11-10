namespace LingoLogger.Data.Models;

public class TogglIntegration
{
    public Guid Id { get; set; }
    public required string WebhookSecret { get; set; }
    public Guid UserId { get; set; }
    public required User User { get; set; }
    public required bool IsVerified {get; set;} = false;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}