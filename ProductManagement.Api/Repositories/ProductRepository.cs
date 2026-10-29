
using Microsoft.EntityFrameworkCore;
using ProductManagement.Api.Models;
using ProductManagement.Api.Data;
using ProductManagement.Api.DTOs;

namespace ProductManagement.Api.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(IReadOnlyCollection<Product> Items, int TotalCount)>
        GetAllAsync(
            ProductQuery request,
            int businessId,
            CancellationToken ct)
    {
        // Only retrieve products belonging to this business.
        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.CategoryRecord)
            .Where(p => p.BusinessId == businessId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();

            query = query.Where(p =>
                p.Name.Contains(term) ||
                p.Sku.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var category = request.Category.Trim();

            query = query.Where(p => p.Category == category);
        }

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice.Value);

        if (request.IsActive.HasValue)
            query = query.Where(p => p.IsActive == request.IsActive.Value);

        var sortBy = request.SortBy?.Trim();
        var sortDirection = request.SortDirection?.Trim();

        query = (sortBy?.ToLowerInvariant(), sortDirection?.ToLowerInvariant()) switch
        {
            ("price", "desc")   => query.OrderByDescending(p => p.Price),
            ("price", _)        => query.OrderBy(p => p.Price),

            ("created", "desc") => query.OrderByDescending(p => p.CreatedAtUtc),
            ("created", _)      => query.OrderBy(p => p.CreatedAtUtc),

            ("name", "desc")    => query.OrderByDescending(p => p.Name),
            _                   => query.OrderBy(p => p.Name)
        };

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public Task<Product?> GetByIdAsync(
        int id,
        int businessId,
        CancellationToken ct)
    {
        return _db.Products
            .Include(p => p.CategoryRecord)
            .FirstOrDefaultAsync(
            p => p.Id == id && p.BusinessId == businessId,
            ct);
    }

    public Task<Product?> GetBySkuAsync(
        string sku,
        int businessId,
        CancellationToken ct)
    {
        return _db.Products.FirstOrDefaultAsync(
            p => p.Sku == sku && p.BusinessId == businessId,
            ct);
    }

    public async Task AddAsync(Product product, CancellationToken ct)
    {
        await _db.Products.AddAsync(product, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Product product, CancellationToken ct)
    {
        _db.Products.Remove(product);
        await _db.SaveChangesAsync(ct);
    }
}
