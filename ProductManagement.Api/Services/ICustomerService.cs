using ProductManagement.Api.DTOs;

namespace ProductManagement.Api.Services;

public interface ICustomerService
{
    Task<PagedResult<CustomerResponseDto>> GetAllAsync(CustomerQuery request, CancellationToken ct);
    Task<CustomerResponseDto?> GetByIdAsync(int Id, CancellationToken ct);
    Task<CustomerResponseDto> CreateAsync(CreateCustomerDTO request, CancellationToken ct);
    Task<bool> UpdateAsync(int Id, UpdateCustomerDTO request, CancellationToken ct);
    Task<bool> DeleteAsync(int Id, CancellationToken ct);
}