using System.Text.Json.Serialization;
using Cirrus.Models.Business.Album;
using Cirrus.Models.Business.Lyrics;
using Cirrus.Models.Business.Playlist;
using Cirrus.Models.Business.Track;
using Cirrus.Models.Serialization.Converters;
using Cirrus.Models.Shared.User;

namespace Cirrus.Models.Serialization;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    Converters = [
        typeof(FlexibleLongConverter)
    ]
)]
[JsonSerializable(typeof(ulong[]))]
[JsonSerializable(typeof(OnlineTrack))]
[JsonSerializable(typeof(TrackLyrics))]
[JsonSerializable(typeof(OnlinePlaylist))]
[JsonSerializable(typeof(OnlineAlbum))]
[JsonSerializable(typeof(OnlineAlbumBrief))]
[JsonSerializable(typeof(UserProfile))]
public partial class NativeModelSerializationContext : JsonSerializerContext;