using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Api.DTOs;

public class UpdateSupplierDto
{
    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [StringLength(30)]
    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;
}
