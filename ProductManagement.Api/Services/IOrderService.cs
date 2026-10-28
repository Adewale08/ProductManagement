using ProductManagement.Api.DTOs;

namespace ProductManagement.Api.Services;

public interface IOrderService
{
    Task<OrderResponseDto> CreateAsync(
        CreateOrderDto request,
        string userId,
        CancellationToken ct);

    Task<IReadOnlyCollection<OrderResponseDto>> GetAllAsync(
        string userId,
        CancellationToken ct);

    Task<OrderResponseDto?> GetByIdAsync(
        int id,
        string userId,
        CancellationToken ct);

    Task<OrderResponseDto> UpdateStatusAsync(
        int id,
        UpdateOrderStatusDto request,
        string userId,
        CancellationToken ct);
}
