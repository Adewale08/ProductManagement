using ProductManagement.Api.DTOs;

namespace ProductManagement.Api.Services;

public interface ISupplierService
{
    Task<List<SupplierResponseDto>> GetAllAsync(CancellationToken ct);
    Task<SupplierResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<SupplierResponseDto> CreateAsync(
        CreateSupplierDto request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}