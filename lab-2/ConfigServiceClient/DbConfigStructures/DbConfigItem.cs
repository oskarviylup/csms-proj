using System.Text.Json.Serialization;

namespace ConfigServiceClient.DbConfigStructures;

public abstract record DbConfigItem(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("value")] string Value);
