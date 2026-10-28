using Microsoft.AspNetCore.Identity;

namespace ProductManagement.Api.Models;

public class ApplicationUser : IdentityUser
{
    public ICollection<Business> Businesses { get; set; }
        = new List<Business>();
}
