using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Api.DTOs;

public record OrderResponseDto(
    int Id,
    List<OrderItemResponseDto> OrderItems,
    DateTime OrderDate,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);