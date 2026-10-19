namespace ProductManagement.Api.Models;

public class Supplier
{
     public int Id { get; set; }
     public string Name { get; set; } = string.Empty;
     public string Email { get; set; } = string.Empty;
     public string? PhoneNumber { get; set; }
     public bool IsActive { get; set; } = true;
     public ICollection<Product> Products { get; set; } 
        = new List<Product>();
        
}