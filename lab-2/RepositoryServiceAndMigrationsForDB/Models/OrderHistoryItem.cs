using System.Text.Json;
using System.Text.Json.Serialization;
using Task3.Models.Payloads;

namespace Task3.Models;

public class OrderHistoryItem
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public DateTime CreatedAt { get; set; }

    public OrderHistoryItemKind Kind { get; set; }

    public required PayloadBase Payload { get; set; }

    public string PayloadJson
    {
        get => JsonSerializer.Serialize(Payload, Payload.GetType(), _jsonSerializerOptions);
        set => Payload = JsonSerializer.Deserialize<PayloadBase>(value, _jsonSerializerOptions) ?? throw new InvalidOperationException();
    }

    public static PayloadBase? DeserializePayload(string json, OrderHistoryItemKind kind)
    {
        return kind switch
        {
            OrderHistoryItemKind.Created => JsonSerializer.Deserialize<OrderCreatedPayload>(json),
            OrderHistoryItemKind.ItemAdded => JsonSerializer.Deserialize<ItemAddedPayload>(json),
            OrderHistoryItemKind.ItemRemoved => JsonSerializer.Deserialize<ItemRemovedPayload>(json),
            OrderHistoryItemKind.StateChanged => JsonSerializer.Deserialize<StateChangedPayload>(json),
            _ => throw new NotSupportedException($"Unsupported kind: {kind}"),
        };
    }

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}