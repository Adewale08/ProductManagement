using ProductManagement.Api.DTOs;

namespace ProductManagement.Api.Services;

public interface IProductService
{
    Task<IReadOnlyCollection<ProductResponseDto>> GetAllAsync(
        string userId, CancellationToken ct);

    Task<ProductResponseDto?> GetByIdAsync(
        int id, string userId, CancellationToken ct);

    Task<ProductResponseDto> CreateAsync(
        CreateProductDto request, string userId, CancellationToken ct);

    Task<bool> UpdateAsync(
        int id, UpdateProductDto request, string userId, CancellationToken ct);

    Task<bool> DeleteAsync(
        int id, string userId, CancellationToken ct);

    Task<bool> UpdateStockAsync(
        int id, int stockQuantity, string userId, CancellationToken ct);
}
