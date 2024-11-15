using Discord;
using Discord.Interactions;
using LingoLogger.Data.Access;
using LingoLogger.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot;

// The derived class to filter for 'Readable' log type
public class ReadingMediumAutocompleteHandler(ILogger<ReadingMediumAutocompleteHandler> logger, LingoLoggerDbContext dbContext) : MediumAutocompleteHandler(logger, dbContext)
{
    // Enforcing specific LogType for this handler
    protected override LogType Filter => LogType.Readable;
}

// The derived class to filter for 'Readable' log type
public class AudibleMediumAutocompleteHandler(ILogger<AudibleMediumAutocompleteHandler> logger, LingoLoggerDbContext dbContext) : MediumAutocompleteHandler(logger, dbContext)
{
    // Enforcing specific LogType for this handler
    protected override LogType Filter => LogType.Audible;
}


// The derived class to filter for 'Readable' log type
public class WatchableMediumAutocompleteHandler(ILogger<WatchableMediumAutocompleteHandler> logger, LingoLoggerDbContext dbContext) : MediumAutocompleteHandler(logger, dbContext)
{
    // Enforcing specific LogType for this handler
    protected override LogType Filter => LogType.Watchable;
}

// The base handler, allowing derived classes to specify different filters
public abstract class MediumAutocompleteHandler(ILogger<MediumAutocompleteHandler> logger, LingoLoggerDbContext dbContext)
    : AutocompleteHandler
{
    // Using null checks for dependencies
    protected readonly ILogger Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    protected readonly LingoLoggerDbContext DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    // Abstract property to enforce setting LogType filter in derived classes
    protected abstract LogType Filter { get; }

    /// <summary>
    /// Generates autocomplete suggestions based on the specified filter.
    /// </summary>
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        try
        {
            var media = await DbContext.Media
                .Where(m => m.LogType == Filter || m.LogType == LogType.Other)
                .Select(m => new AutocompleteResult(m.Name, m.Name))
                .ToListAsync();

            return AutocompletionResult.FromSuccess(media);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to generate autocomplete suggestions for filter {Filter}", Filter);
            return AutocompletionResult.FromError(ex);
        }
    }
}
