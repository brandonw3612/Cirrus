using System.Text.Json.Serialization;
using Cirrus.Models.Business.Lyrics;
using Cirrus.Models.Business.Track;

namespace Cirrus.Models.Serialization;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(ulong[]))]
[JsonSerializable(typeof(OnlineTrack))]
[JsonSerializable(typeof(TrackLyrics))]
public partial class NativeModelSerializationContext : JsonSerializerContext;