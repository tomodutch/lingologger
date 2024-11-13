namespace LingoLogger.Data.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public ICollection<Log> Logs { get; set; } = [];
        public ICollection<Goal> Goals { get; set; } = [];
        public ulong? DiscordId { get; set; }
        public ICollection<TogglIntegration> TogglIntegrations { get; set; } = [];
        public ICollection<Milestone> Milestones { get; set; } = [];
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }
}
