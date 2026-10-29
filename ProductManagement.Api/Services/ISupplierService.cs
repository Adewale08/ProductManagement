using ProductManagement.Api.DTOs;

namespace ProductManagement.Api.Services;

public interface ISupplierService
{
    Task<List<SupplierResponseDto>> GetAllAsync(string userId, CancellationToken ct);
    Task<SupplierResponseDto?> GetByIdAsync(int id, string userId, CancellationToken ct);
    Task<SupplierResponseDto> CreateAsync(
        CreateSupplierDto request, string userId, CancellationToken ct);
    Task<bool> UpdateAsync(int id, UpdateSupplierDto request, string userId, CancellationToken ct);
    Task<bool> DeleteAsync(int id, string userId, CancellationToken ct);
}
