using System.Text.Json.Serialization;
using Cirrus.Models.Business.Lyrics;
using Cirrus.Models.Business.Playlist;
using Cirrus.Models.Business.Track;

namespace Cirrus.Models.Serialization;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(ulong[]))]
[JsonSerializable(typeof(OnlineTrack))]
[JsonSerializable(typeof(TrackLyrics))]
[JsonSerializable(typeof(OnlinePlaylist))]
public partial class NativeModelSerializationContext : JsonSerializerContext;