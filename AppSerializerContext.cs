using System.Text.Json.Serialization;

namespace DotnetRedfish;

[JsonSerializable(typeof(Headers))]
internal partial class AppSerializerContext : JsonSerializerContext { }
