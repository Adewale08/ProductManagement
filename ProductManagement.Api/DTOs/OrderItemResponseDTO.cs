using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Api.DTOs;
public record OrderItemResponseDto(
    int Id,
    int ProductId,
    int Quantity,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);