using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Artist;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;

namespace Cirrus.Network.Api;

partial class Artist
{
    /// <summary>
    /// Gets description for the specified artist.
    /// API Route: /api/artist/description.
    /// </summary>
    [MusicApi("description")]
    internal MusicApi<ulong, ArtistDescriptionApiResponse> DescriptionApi => field ??= new(
        $"{RouteBase}/description",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/weapi/artist/introduction",
        static p => new()
        {
            ["id"] = p
        }
    );

    /// <summary>
    /// Gets description for the specified artist.
    /// </summary>
    /// <param name="artistId">Id of the artist.</param>
    /// <returns>Description for the specified artist.</returns>
    public Task<ArtistDescriptionApiResponse> GetDescriptionAsync(ulong artistId) => DescriptionApi.RequestAsync(artistId);
}