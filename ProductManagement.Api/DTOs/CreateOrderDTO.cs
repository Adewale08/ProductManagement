using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Api.DTOs;

public class CreateOrderDTO
{
    [Required]
    public List<CreateOrderItemDTO> OrderItems { get; set; } = new List<CreateOrderItemDTO>();

    [Required]
    public DateTime OrderDate { get; set; }
}