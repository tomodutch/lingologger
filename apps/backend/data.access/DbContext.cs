using LingoLogger.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LingoLogger.Data.Access
{
    public class LingoLoggerDbContext(DbContextOptions<LingoLoggerDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<TogglIntegration> TogglIntegrations { get; set; }
        public DbSet<Milestone> Milestones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Log>(log =>
            {
                log.HasDiscriminator<string>("LogType")
                    .HasValue<ReadableLog>("Readable")
                    .HasValue<AudibleLog>("Audible")
                    .HasValue<WatchableLog>("Watchable")
                    .HasValue<EpisodicLog>("Episodic");

                log.HasQueryFilter(log => log.DeletedAt == null);
                log.HasKey(l => new { l.UserId, l.CreatedAt });
                log.HasIndex(l => l.UserId);
                log.Property(l => l.UserId).IsRequired();
                log.Property(l => l.Title).IsRequired().HasMaxLength(100);
                log.Property(l => l.Medium).IsRequired().HasMaxLength(100);
                log.Property(l => l.Source).IsRequired().HasMaxLength(100);
                log.Property(l => l.AmountOfSeconds).IsRequired().HasMaxLength(60 * 60 * 24);
                log.Property(l => l.Coefficient).IsRequired().HasMaxLength(60 * 60 * 24);
                log.Property(l => l.SourceEventId).HasMaxLength(100);
                log.Property(l => l.CreatedAt)
                      .IsRequired()
                      .HasColumnType("timestamptz")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
                log.Property(l => l.DeletedAt)
                      .IsRequired(false)
                      .HasColumnType("timestamptz");
                log.HasOne(l => l.User)
                    .WithMany(u => u.Logs)
                    .HasForeignKey(l => l.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                log.HasIndex(l => new { l.UserId, l.SourceEventId });
            });

            // Configure User entity
            modelBuilder.Entity<User>(user =>
            {
                // Primary Key
                user.HasKey(u => u.Id);
                user.HasQueryFilter(user => user.DeletedAt == null);
                // Properties
                user.Property(u => u.Id)
                      .IsRequired()
                      .ValueGeneratedOnAdd(); // GUID generated on add

                user.Property(u => u.DiscordId).IsRequired(false);
                user.HasIndex(u => u.DiscordId);

                // Relationships
                user.HasMany(u => u.Logs)
                      .WithOne(l => l.User)
                      .HasForeignKey(l => l.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // Cascade delete if a user is deleted
                user.Property(l => l.CreatedAt)
                      .IsRequired()
                      .HasColumnType("timestamptz")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
            });

            modelBuilder.Entity<Goal>(goal =>
            {
                goal.ToTable("Goals");
                goal.HasQueryFilter(g => g.DeletedAt == null);
                goal.HasQueryFilter(g => g.User.DeletedAt == null);
                goal.HasKey(g => g.Id);
                goal.HasIndex(g => g.UserId);
                goal.HasOne(g => g.User)
                    .WithMany(u => u.Goals)
                    .HasForeignKey(g => g.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
                goal.Property(g => g.EndsAt).IsRequired().HasColumnType("timestamptz");
                goal.Property(g => g.TargetTimeInSeconds).IsRequired();
                goal.Property(l => l.DeletedAt)
                      .IsRequired(false)
                      .HasColumnType("timestamptz");
                goal.Property(l => l.CreatedAt)
                      .IsRequired()
                      .HasColumnType("timestamptz")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
            });

            modelBuilder.Entity<TogglIntegration>(e =>
            {
                e.ToTable("toggleIntegrations");
                e.HasQueryFilter(e => e.DeletedAt == null);
                e.HasKey(e => e.Id);
                e.Property(e => e.WebhookSecret).HasMaxLength(50);
                e.Property(e => e.IsVerified).HasDefaultValue(false);
                e.HasOne(e => e.User)
                    .WithMany(u => u.TogglIntegrations)
                    .HasForeignKey(e => e.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                e.Property(e => e.CreatedAt)
                      .IsRequired()
                      .HasColumnType("timestamptz")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
                e.Property(e => e.DeletedAt).IsRequired(false).HasColumnType("timestamptz");
            });

            modelBuilder.Entity<Milestone>(e =>
            {
                e.ToTable("milestones");
                e.HasQueryFilter(e => e.DeletedAt == null);
                e.HasKey(e => e.Id);
                e.Property(e => e.Title).HasMaxLength(255);
                e.HasOne(e => e.User)
                    .WithMany(u => u.Milestones)
                    .HasForeignKey(e => e.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                e.Property(e => e.CreatedAt)
                      .IsRequired()
                      .HasColumnType("timestamptz")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
                e.Property(e => e.ReachedAt).IsRequired(false).HasColumnType("timestamptz");
                e.Property(e => e.DeletedAt).IsRequired(false).HasColumnType("timestamptz");

                e.HasIndex(e => e.UserId);
            });
        }
    }
}
