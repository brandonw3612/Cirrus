using System.Text.Json.Serialization;
using Cirrus.Models.Network.Album;

namespace Cirrus.Models.Network.Response.Public;

// TODO: Documentation needed.

public class NewAlbumsApiResponse : MusicApiResponse
{
    [JsonPropertyName("albums")] public List<AlbumDetail> Albums { get; init; } = new();
}