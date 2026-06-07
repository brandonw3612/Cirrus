using System.Text.Json;
using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Track;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using Cirrus.Network.Serialization;

namespace Cirrus.Network.Api;

partial class Track
{
    /// <summary>
    /// Gets details for specified tracks.
    /// API Route: /api/track/details.
    /// </summary>
    [MusicApi("details")]
    internal MusicApi<List<ulong>, TrackDetailsApiResponse> DetailsApi => field ??= new(
        $"{RouteBase}/details",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/api/v3/song/detail",
        p => new()
        {
            ["c"] = JsonSerializer.Serialize<Dictionary<string, ulong>[]>(
                p.Select(id => new Dictionary<string, ulong> { ["id"] = id}).ToArray(),
                NetworkSerializationContext.Default.DictionaryStringUInt64Array
            )
        }
    );

    /// <summary>
    /// Gets details for specified tracks.
    /// </summary>
    /// <param name="trackIds">IDs of the tracks.</param>
    /// <returns>Details for specified tracks.</returns>
    public Task<TrackDetailsApiResponse> GetDetailsAsync(List<ulong> trackIds) => DetailsApi.RequestAsync(trackIds);
}