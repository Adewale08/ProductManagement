using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Api.DTOs;

public class CreateOrderItemDTO
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    public int Quantity { get; set; }
}