using System.Text.Json.Serialization;

namespace Cirrus.Serialization;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Utilities.Primitives.KeyboardShortcut[]))]
[JsonSerializable(typeof(Utilities.Primitives.QuickAccess))]
public partial class AppSerializationContext : JsonSerializerContext;