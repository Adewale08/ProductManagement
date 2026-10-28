namespace ProductManagement.Api.DTOs;

public record OrderItemResponseDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
