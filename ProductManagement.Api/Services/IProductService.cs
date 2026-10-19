using ProductManagement.Api.DTOs;

namespace ProductManagement.Api.Services;

public interface IProductService
{
    Task<IReadOnlyCollection<ProductResponseDto>> GetAllAsync(CancellationToken ct);
    Task<ProductResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<ProductResponseDto> CreateAsync(CreateProductDto request, CancellationToken ct);
    Task<bool> UpdateAsync(int id, UpdateProductDto request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<bool> UpdateStockAsync(
    int id,
    int stockQuantity,
    CancellationToken ct);
}

