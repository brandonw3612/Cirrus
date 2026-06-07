using System.Text.Json.Serialization;

namespace Cirrus.Models.Network.Response.Search.GeneralSearchModules;

/// <summary>
/// Similar query module in the general search result.
/// </summary>
public class SimilarQuerySuggestionsModule : SearchModuleBase, IJsonOnDeserialized
{
    /// <summary>
    /// Similar query keywords related to the keyword.
    /// </summary>
    [JsonIgnore] public List<string> Keywords { get; private set; } = new();

    [JsonInclude] [JsonPropertyName("sim_querys")]
    internal List<QuerySuggestionEntry> SuggestionEntries { get; init; } = new();
    
    void IJsonOnDeserialized.OnDeserialized()
    {
        Keywords = SuggestionEntries.Select(e => e.Keyword).OfType<string>().ToList();
    }
    
    public class QuerySuggestionEntry
    {
        [JsonPropertyName("keyword")] public string? Keyword { get; init; }
    }
}