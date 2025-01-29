using System.Text.Json.Serialization;

namespace ConfigServiceClient.DbConfigStructures;

public record DbConfigResponse(
    [property: JsonPropertyName("items")] IEnumerable<DbConfigItem> Items,
    [property: JsonPropertyName("pageToken")] string PageToken);
