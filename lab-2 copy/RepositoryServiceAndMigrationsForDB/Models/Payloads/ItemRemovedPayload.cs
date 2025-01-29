using System.Text.Json.Serialization;

namespace Task3.Models.Payloads;

[JsonDerivedType(typeof(ItemRemovedPayload), "ItemRemovedPayload")]
public class ItemRemovedPayload : PayloadBase
{
    public long ProductId { get; set; }
}