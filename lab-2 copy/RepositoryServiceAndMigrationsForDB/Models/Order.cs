namespace Task3.Models;

public class Order
{
    public long Id { get; set; }

    public OrderState State { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }
}