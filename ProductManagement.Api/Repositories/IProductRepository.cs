
using ProductManagement.Api.Models;
using ProductManagement.Api.DTOs;

namespace ProductManagement.Api.Repositories;

public interface IProductRepository
{
    Task<(IReadOnlyCollection<Product> Items, int TotalCount)>
        GetAllAsync(ProductQuery request, int businessId, CancellationToken ct);

    Task<Product?> GetByIdAsync(int id, int businessId, CancellationToken ct);

    Task<Product?> GetBySkuAsync(string sku, int businessId, CancellationToken ct);

    Task AddAsync(Product product, CancellationToken ct);

    Task UpdateAsync(Product product, CancellationToken ct);

    Task DeleteAsync(Product product, CancellationToken ct);
}
