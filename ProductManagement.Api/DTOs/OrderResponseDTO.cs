namespace ProductManagement.Api.DTOs;

public record OrderResponseDto(
    int Id,
    DateTime CreatedAtUtc,
    string Status,
    decimal TotalAmount,
    IReadOnlyCollection<OrderItemResponseDto> Items);
