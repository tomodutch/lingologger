using LingoLogger.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LingoLogger.Data.Access
{
    public class LingoLoggerDbContext(DbContextOptions<LingoLoggerDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Log>()
                .HasDiscriminator<string>("LogType")
                .HasValue<ReadableLog>("Readable")
                .HasValue<AudibleLog>("Audible")
                .HasValue<WatchableLog>("Watchable");

            modelBuilder.Entity<Log>(log =>
            {
                log.HasKey(l => new {l.UserId, l.CreatedAt});
                log.HasIndex(l => l.UserId);
                log.Property(l => l.UserId).IsRequired();
                log.Property(l => l.Title).IsRequired().HasMaxLength(100);
                log.Property(l => l.Medium).IsRequired().HasMaxLength(100);
                log.Property(l => l.Source).IsRequired().HasMaxLength(100);
                log.Property(l => l.AmountOfSeconds).IsRequired().HasMaxLength(60 * 60 * 24);
                log.Property(l => l.Coefficient).IsRequired().HasMaxLength(60 * 60 * 24);
                log.Property(l => l.CreatedAt)
                      .IsRequired()
                      .HasColumnType("timestamptz")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
                log.Property(l => l.DeletedAt)
                      .IsRequired()
                      .HasColumnType("timestamptz")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
                log.HasOne(l => l.User)
                    .WithMany(u => u.Logs)
                    .HasForeignKey(l => l.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                // Primary Key
                entity.HasKey(u => u.Id);

                // Properties
                entity.Property(u => u.Id)
                      .IsRequired()
                      .ValueGeneratedOnAdd(); // GUID generated on add

                // Relationships
                entity.HasMany(u => u.Logs)
                      .WithOne(l => l.User)
                      .HasForeignKey(l => l.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // Cascade delete if a user is deleted
            });
        }
    }
}
