using ProductManagement.Api.Models;

namespace ProductManagement.Api.Repositories;

public interface ISupplierRepository
{
    Task<List<Supplier>> GetAllAsync(CancellationToken ct);
    Task<Supplier?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct);
    Task<bool> HasAnyProductsAsync(int id, CancellationToken ct);
    Task<bool> HasActiveProductsAsync(int id, CancellationToken ct);
    Task AddAsync(Supplier supplier, CancellationToken ct);
    Task DeleteAsync(Supplier supplier, CancellationToken ct);
}