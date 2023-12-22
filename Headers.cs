using System.Text.Json.Serialization;

namespace DotnetRedfish;

public record Headers([property: JsonPropertyName("GET")] Dictionary<string, string> Get);
