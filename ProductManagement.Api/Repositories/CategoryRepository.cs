using Microsoft.EntityFrameworkCore;
using ProductManagement.Api.Models;
using ProductManagement.Api.Data;
using ProductManagement.Api.DTOs;

namespace ProductManagement.Api.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _dbContext;

    public CategoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IReadOnlyCollection<Category> Items, int TotalCount)> GetAllAsync(CategoryQuery request, CancellationToken ct)
    {
        var query = _dbContext.Categories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            query = query.Where(c => c.Name.Contains(request.Name));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<Category?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _dbContext.Categories.FindAsync(new object[] { id }, ct);
    }

    public async Task<Category> AddAsync(Category category, CancellationToken ct)
    {
        await _dbContext.Categories.AddAsync(category, ct);
        await _dbContext.SaveChangesAsync(ct);
        return category;
    }

    public async Task UpdateAsync(Category category, CancellationToken ct)
    {
        _dbContext.Categories.Update(category);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Category category, CancellationToken ct)
    {
        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync(ct);
    }
}