using System.Text.Json.Serialization;

namespace Task3.Models.Payloads;

[JsonDerivedType(typeof(ItemAddedPayload), "ItemAddedPayload")]
public class ItemAddedPayload : PayloadBase
{
    public long ProductId { get; set; }

    public int Quantity { get; set; }
}