namespace Task3.Models;

public class OrderItem
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public long ProductId { get; set; }

    public int Quantity { get; set; }

    public bool IsDeleted { get; set; }
}