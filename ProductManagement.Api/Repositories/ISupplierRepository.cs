using ProductManagement.Api.Models;

namespace ProductManagement.Api.Repositories;

public interface ISupplierRepository
{
    Task<List<Supplier>> GetAllAsync(int businessId, CancellationToken ct);
    Task<Supplier?> GetByIdAsync(int id, int businessId, CancellationToken ct);
    Task<bool> EmailExistsAsync(string email, int businessId, int? exceptId, CancellationToken ct);
    Task<bool> HasAnyProductsAsync(int id, int businessId, CancellationToken ct);
    Task<bool> HasActiveProductsAsync(int id, int businessId, CancellationToken ct);
    Task AddAsync(Supplier supplier, CancellationToken ct);
    Task UpdateAsync(Supplier supplier, CancellationToken ct);
    Task DeleteAsync(Supplier supplier, CancellationToken ct);
}
