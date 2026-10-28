using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Api.DTOs;

public class CreateOrderDto
{
    [Required, MinLength(1)]
    public List<CreateOrderItemDto> Items { get; set; } = [];
}
