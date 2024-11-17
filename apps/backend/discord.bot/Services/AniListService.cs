using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LingoLogger.Discord.Bot;
public class AniListQuery
{
    private readonly string _baseUrl = "https://graphql.anilist.co";
    private readonly HttpClient _httpClient;

    public AniListQuery()
    {
        _httpClient = new HttpClient();
    }

    public async Task<AniListResponse> SearchAnimeOrManga(string query)
    {
        // GraphQL query to search for anime or manga
        var queryString = @"
        query ($search: String) {
            Media(search: $search, type: ANIME) {
                id
                title {
                    romaji
                    english
                    native
                }
                description
            }
        }";

        var variables = new
        {
            search = query
        };

        // Create the GraphQL query object
        var requestBody = new
        {
            query = queryString,
            variables
        };

        // Serialize the object to a JSON string
        var jsonContent = JsonConvert.SerializeObject(requestBody);

        // Send the request content
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        // Send POST request
        var response = await _httpClient.PostAsync(_baseUrl, content);
        var responseContent = JsonConvert.DeserializeObject<AniListResponse>(await response.Content.ReadAsStringAsync());
        // Parse the response
        return responseContent;
    }
}

public class AniListResponse
{
    [JsonProperty("data")]
    public AniListResponseData Data { get; set; }
}

public class AniListResponseData
{
    [JsonProperty("Media")]
    public Media Media { get; set; }
}

public class Media
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("title")]
    public Title Title { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }
}

public class Title
{
    [JsonProperty("romaji")]
    public string Romaji { get; set; }

    [JsonProperty("english")]
    public string English { get; set; }

    [JsonProperty("native")]
    public string Native { get; set; }
}