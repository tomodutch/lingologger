using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LingoLogger.Discord.Bot.Services;

public class GoogleBookApiService(ILogger<GoogleBookApiService> logger, HttpClient httpClient, IMemoryCache cache)
{
    private readonly ILogger<GoogleBookApiService> _logger = logger;
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<Suggestion>> GetBookSuggestionsAsync(string query)
    {
        string apiUrl = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(query)}&langRestrict=ja";
        string responseBody = "";
        if (cache.TryGetValue(query, out string? cachedResponse) && cachedResponse != null)
        {
            responseBody = cachedResponse;
        }
        else
        {
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }
            responseBody = await response.Content.ReadAsStringAsync();
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                  .SetSize(Encoding.UTF8.GetByteCount(responseBody))
                  .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
            cache.Set(CreateCacheKey(query), responseBody, cacheEntryOptions);
        }

        var bookResponse = JsonConvert.DeserializeObject<BookResponse>(responseBody);
        List<Suggestion> suggestions = [];
        foreach (var item in bookResponse?.Items ?? [])
        {
            if (item.VolumeInfo?.Title != null && item.Id != null)
            {
                suggestions.Add(new()
                {
                    Id = item.Id,
                    Title = item.VolumeInfo.Title
                });
            }
        }


        return suggestions;
    }

    private string CreateCacheKey(string query)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(query));
        return BitConverter.ToString(hashBytes).ToLower();
    }
}

public class Suggestion
{
    public required string Id { get; set; }
    public required string Title { get; set; }
}

public class BookResponse
{
    [JsonProperty("items")]
    public required List<BookItem> Items { get; set; }
}

public class BookItem
{
    [JsonProperty("id")]
    public required string Id { get; set; }
    [JsonProperty("volumeInfo")]
    public required VolumeInfo VolumeInfo { get; set; }
}

public class VolumeInfo
{
    [JsonProperty("title")]
    public required string Title { get; set; }

    [JsonProperty("thumbnail")]
    public required string Thumbnail { get; set; }
}