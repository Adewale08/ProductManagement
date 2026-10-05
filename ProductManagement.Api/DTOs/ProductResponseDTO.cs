using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Api.DTOs;

public record ProductResponseDto(
    int Id,
    string Name,
    string? Description,
    string Sku,
    string Category,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);