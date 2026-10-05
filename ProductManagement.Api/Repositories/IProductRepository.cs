using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProductManagement.Api.Models;
using ProductManagement.Api.DTOs;

namespace ProductManagement.Api.Repositories;

public interface IProductRepository
{
	Task<(IReadOnlyCollection<Product> Items, int TotalCount)> GetAllAsync(ProductManagement.Api.DTOs.ProductQuery request, CancellationToken ct);
	Task<Product?> GetByIdAsync(int id, CancellationToken ct);
	Task<Product?> GetBySkuAsync(string sku, CancellationToken ct);
	Task AddAsync(Product product, CancellationToken ct);
	Task UpdateAsync(Product product, CancellationToken ct);
	Task DeleteAsync(Product product, CancellationToken ct);
}