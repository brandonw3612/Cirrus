using Cirrus.Generated.Attributes;
using Cirrus.Models.Network.Response.Playlist;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using CreateApiParameter = (string Title, bool? IsPrivate, Cirrus.Network.Api.Playlist.CreateApiParameterPlaylistType? Type);

namespace Cirrus.Network.Api;

partial class Playlist
{
    /// <summary>
    /// Special type specified when creating a playlist.
    /// </summary>
    public enum CreateApiParameterPlaylistType
    {
        /// <summary>
        /// Video Playlist.
        /// </summary>
        Video,
        /// <summary>
        /// Shared Playlist.
        /// </summary>
        Shared,
        /// <summary>
        /// Normal Playlist.
        /// </summary>
        Normal
    }

    /// <summary>
    /// Creates a playlist for current user.
    /// API Route: /api/playlist/create.
    /// </summary>
    [MusicApi("create")]
    internal MusicApi<CreateApiParameter, PlaylistCreateApiResponse> CreateApi => field ??= new(
        $"{RouteBase}/create",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = WeapiHandler.Current
        },
        $"{Constants.RequestBase}/api/playlist/create",
        p => new()
        {
            ["name"] = p.Title,
            ["privacy"] = p.IsPrivate is true ? 10 : 0,
            ["type"] = p.Type switch
            {
                CreateApiParameterPlaylistType.Normal => "NORMAL",
                CreateApiParameterPlaylistType.Video => "VIDEO",
                CreateApiParameterPlaylistType.Shared => "SHARED",
                _ => "NORMAL"
            }
        }
    );

    /// <summary>
    /// Creates a playlist for current user.
    /// </summary>
    /// <param name="title">Title of the playlist.</param>
    /// <param name="isPrivate">Whether the playlist is a private one.</param>
    /// <param name="type">Special type of the playlist.</param>
    /// <returns>Created playlist.</returns>
    public Task<PlaylistCreateApiResponse> CreateAsync(string title, bool? isPrivate = null,
        CreateApiParameterPlaylistType? type = null) =>
        CreateApi.RequestAsync((title, isPrivate, type));
}