using System.Text.Json.Serialization;
using Cirrus.Models.Network.Artist;

namespace Cirrus.Models.Network.Response.Artist;

/// <summary>
/// Response for API artist/details.
/// </summary>
public class ArtistDetailsApiResponse : MusicApiResponse, IJsonOnDeserialized
{
    /// <summary>
    /// Total count of the artist's videos.
    /// </summary>
    [JsonIgnore] public int VideoCount { get; private set; }
    
    /// <summary>
    /// Detail of the artist.
    /// </summary>
    [JsonIgnore] public ArtistDetail2? Artist { get; private set; }
    
    /// <summary>
    /// Whether the current user blocks the current artist.
    /// </summary>
    [JsonIgnore] public bool IsBlocked { get; private set; }

    /// <summary>
    /// Artist's identities in different aspects of the music industry.
    /// </summary>
    [JsonIgnore] public List<ArtistIdentity> Identities { get; private set; } = new();

    [JsonInclude] [JsonPropertyName("data")] internal DetailApiResponseData? Data { get; init; }
    
    void IJsonOnDeserialized.OnDeserialized()
    {
        if (Data is null) return;
        VideoCount = Data.VideoCount;
        Artist = Data.Artist;
        IsBlocked = Data.IsBlocked;
        Identities = Data.Identities;
    }
    
    public class DetailApiResponseData
    {
        [JsonPropertyName("videoCount")] public int VideoCount { get; init; }
        [JsonPropertyName("artist")] public ArtistDetail2? Artist { get; init; }
        [JsonPropertyName("blacklist")] public bool IsBlocked { get; init; } 
        [JsonPropertyName("secondaryExpertIdentiy")] public List<ArtistIdentity> Identities { get; init; } = new();
    }
}

