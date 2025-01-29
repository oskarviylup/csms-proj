using System.Text.Json.Serialization;

namespace Task3.Models.Payloads;

[JsonDerivedType(typeof(OrderCreatedPayload), "OrderCreatedPayload")]
public class OrderCreatedPayload : PayloadBase
{
    public DateTime CreatedAt { get; set; }
}