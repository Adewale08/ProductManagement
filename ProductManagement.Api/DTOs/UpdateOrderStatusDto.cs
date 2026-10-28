using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Api.DTOs;

public class UpdateOrderStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
