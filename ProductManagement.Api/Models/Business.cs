namespace ProductManagement.Api.Models;

public class Business
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string OwnerId { get; set; } = string.Empty;
    public ApplicationUser Owner { get; set; } = null!;
    public ICollection<Product> Products { get; set; }
        = new List<Product>();
    public ICollection<Supplier> Suppliers { get; set; }
        = new List<Supplier>();
    public ICollection<Category> Categories { get; set; }
        = new List<Category>();
}
