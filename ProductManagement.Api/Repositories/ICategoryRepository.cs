using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProductManagement.Api.Models;
using ProductManagement.Api.DTOs;

namespace ProductManagement.Api.Repositories;

public interface ICategoryRepository
{
    Task<(IReadOnlyCollection<Category> Items, int TotalCount)> GetAllAsync(CategoryQuery request, int businessId, CancellationToken ct);
    Task<Category?> GetByIdAsync(int id, int businessId, CancellationToken ct);
    Task<Category> AddAsync(Category category, CancellationToken ct);
    Task UpdateAsync(Category category, CancellationToken ct);
    Task DeleteAsync(Category category, CancellationToken ct);
    Task<bool> IsUsedByProductsAsync(int id, int businessId, CancellationToken ct);
}
