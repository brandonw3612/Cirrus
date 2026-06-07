using System.Net;
using System.Text.Json.Serialization;

namespace Cirrus.Base.Serialization;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(List<Cookie>))]
[JsonSerializable(typeof(double[]))]
internal partial class SerializationContext : JsonSerializerContext;