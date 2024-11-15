using LingoLogger.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LingoLogger.Data.Access
{
    public class LingoLoggerDbContext(DbContextOptions<LingoLoggerDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<Medium> Media { get; set; }
        public DbSet<TogglIntegration> TogglIntegrations { get; set; }
        public DbSet<Milestone> Milestones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var logTypeConverter = new ValueConverter<LogType, string>(
                v => LogTypeConverter.ConvertLogTypeToString(v),
                v => LogTypeConverter.ConvertStringToLogType(v)
            );

            modelBuilder.Entity<Log>(log =>
            {
                log.HasQueryFilter(log => log.DeletedAt == null);
                log.HasKey(l => new { l.UserId, l.CreatedAt });
                log.HasIndex(l => l.UserId);
                log.Property(l => l.UserId).IsRequired();
                log.Property(l => l.LogType).IsRequired().HasConversion(logTypeConverter);
                log.Property(l => l.Title).IsRequired().HasMaxLength(100);
                log.Property(l => l.MediumId).IsRequired(false);
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

                log.HasOne(l => l.Medium)
                    .WithMany(m => m.Logs)
                    .HasForeignKey(l => l.MediumId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                log.HasIndex(l => new { l.UserId, l.SourceEventId });
                log.HasIndex(l => new { l.UserId, l.MediumId });
                log.HasIndex(l => l.MediumId);
            });
            modelBuilder.Entity<Medium>(medium =>
            {
                medium.ToTable("Media");
                medium.HasKey(m => m.Id);
                medium.HasQueryFilter(user => user.DeletedAt == null);
                medium.Property(m => m.LogType).IsRequired().HasConversion(logTypeConverter);
                medium.Property(m => m.Name).IsRequired().HasMaxLength(25);
                medium.Property(m => m.GuildId).IsRequired(false);
                medium.HasMany(m => m.Logs)
                      .WithOne(l => l.Medium)
                      .HasForeignKey(l => l.MediumId)
                      .OnDelete(DeleteBehavior.Cascade);
                medium.Property(l => l.CreatedAt)
                      .IsRequired()
                      .HasColumnType("timestamptz")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
                medium.Property(l => l.DeletedAt).IsRequired(false).HasColumnType("timestamptz");
            });
            modelBuilder.Entity<Medium>().HasData(
                new Medium
                {
                    Id = Guid.Parse("c732a6f7-ccc6-44ad-88aa-e1da57e16c4a"),
                    Name = "Book",
                    LogType = LogType.Readable,
                },
                new Medium
                {
                    Id = Guid.Parse("46906a9f-1062-45fc-b5cd-8b30fa97062a"),
                    Name = "Visual Novel",
                    LogType = LogType.Readable,
                },
                new Medium
                {
                    Id = Guid.Parse("8b51cd17-c2f0-4b4b-99ee-63c99924d107"),
                    Name = "Podcast",
                    LogType = LogType.Audible,
                },
                new Medium
                {
                    Id = Guid.Parse("e11ca2ad-9903-4926-a21e-6640fac79089"),
                    Name = "Audiobook",
                    LogType = LogType.Audible,
                },
                new Medium
                {
                    Id = Guid.Parse("676fb136-e379-4c6a-ad6a-b4aaad2e413e"),
                    Name = "Anime",
                    LogType = LogType.Watchable,
                },
                new Medium
                {
                    Id = Guid.Parse("21daae4a-3dfc-4f8b-b525-0a513376de1a"),
                    Name = "Youtube",
                    LogType = LogType.Watchable,
                },
                new Medium
                {
                    Id = Guid.Parse("bb2adabb-1271-468f-af03-b8457e3e6488"),
                    Name = "Anki",
                    LogType = LogType.Anki,
                },
                new Medium
                {
                    Id = Guid.Parse("4bce7a21-3b1d-4ffd-a7eb-ea50724bf629"),
                    Name = "Other",
                    LogType = LogType.Other,
                }
            );

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
