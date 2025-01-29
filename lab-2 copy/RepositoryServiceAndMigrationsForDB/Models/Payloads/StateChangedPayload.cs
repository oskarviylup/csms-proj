using System.Text.Json.Serialization;

namespace Task3.Models.Payloads;

[JsonDerivedType(typeof(StateChangedPayload), "StateChangedPayload")]
public class StateChangedPayload : PayloadBase
{
    public OrderState OldState { get; set; }

    public OrderState NewState { get; set; }
}