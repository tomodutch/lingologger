using Microsoft.EntityFrameworkCore;
using Grafana.OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;

namespace LingoLogger.Web.Api;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();
        builder.Services.AddHttpClient();
        builder.Services.AddHttpClient("ChartApiClient", client =>
        {
            var uri = builder.Configuration.GetValue<string>("ChartApiUri");
            client.BaseAddress = new Uri(uri);
        });

        builder.Services.AddDbContext<LingoLogger.Data.Access.LingoLoggerDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DbConnection"));
        });
        builder.Services.AddOpenTelemetry().WithTracing(configure =>
        {
            configure.UseGrafana();
        })
        .WithMetrics(configure =>
        {
            configure.UseGrafana();
        });

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.UseGrafana();
        });

        var app = builder.Build();
        app.MapControllers();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.Run();
    }
}
