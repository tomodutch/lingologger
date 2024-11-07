using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LingoLogger.Data.Access
{
    public class LingoLoggerDbContextFactory : IDesignTimeDbContextFactory<LingoLoggerDbContext>
    {
        public LingoLoggerDbContext CreateDbContext(string[] args)
        {
            // Set up configuration to retrieve connection string
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            // Fetch the connection string from appsettings.json
            var connectionString = configuration.GetConnectionString("DbConnection");

            // Set up DbContextOptions with the connection string
            var optionsBuilder = new DbContextOptionsBuilder<LingoLoggerDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new LingoLoggerDbContext(optionsBuilder.Options);
        }
    }
}
