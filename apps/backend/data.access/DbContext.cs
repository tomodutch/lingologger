using LingoLogger.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LingoLogger.Data.Access
{
    public class LingoLoggerDbContext(DbContextOptions<LingoLoggerDbContext> options) : DbContext(options)
    {
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Log>().ToTable("Logs");
            // modelBuilder.Entity<Log>().HasIndex(l => l.Id)
        }
    }
}
