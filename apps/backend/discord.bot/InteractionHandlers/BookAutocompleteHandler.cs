using Discord;
using Discord.Interactions;
using LingoLogger.Data.Access;
using LingoLogger.Discord.Bot.Services;
using Microsoft.Extensions.Logging;

namespace LingoLogger.Discord.Bot.InteractionHandlers;

public class BookAutocompleteHandler(ILogger<BookAutocompleteHandler> logger, GoogleBookApiService bookService) : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        try
        {
            string query = (string)autocompleteInteraction.Data.Current.Value;
            if (string.IsNullOrEmpty(query) && query.Length >= 3)
            {
                return AutocompletionResult.FromSuccess([]);
            }

            var suggestions = await bookService.GetBookSuggestionsAsync(query: query);
            var result = suggestions
                .Take(5)
                .Select(s =>
                {
                    // Max size of autocomplete results is 100 in length
                    if (s.Title.Length >= 100)
                    {
                        return string.Concat(s.Title.AsSpan(0, 98), "…");
                    }
                    return s.Title;
                })
                .ToList();

            return AutocompletionResult.FromSuccess(result.Select(r => new AutocompleteResult(r, r)));
        }
        catch (Exception ex)
        {
            logger.LogError("Autocomplete failed", ex);
            return AutocompletionResult.FromError(ex);
        }

    }
}