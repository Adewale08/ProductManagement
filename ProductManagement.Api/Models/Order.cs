
namespace ProductManagement.Api.Models;

public class Order
{
    public int Id { get; set; }

    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "Pending";

    public decimal TotalAmount { get; set; }

    public ICollection<OrderItem> Items { get; set; }
        = new List<OrderItem>();
}
