using System.Text.Json.Serialization;
using Cirrus.Models.Business.Track;

namespace Cirrus.Models.Serialization;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(ulong[]))]
[JsonSerializable(typeof(OnlineTrack))]
public partial class NativeModelSerializationContext : JsonSerializerContext;