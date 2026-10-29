using Microsoft.EntityFrameworkCore;
using ProductManagement.Api.Data;
using ProductManagement.Api.Models;
namespace ProductManagement.Api.Repositories;
public class SupplierRepository : ISupplierRepository
{
    private readonly AppDbContext _db;
    public SupplierRepository(AppDbContext db) => _db = db;
    public Task<List<Supplier>> GetAllAsync(int businessId, CancellationToken ct)
        => _db.Suppliers.AsNoTracking()
            .Where(s => s.BusinessId == businessId)
            .OrderBy(s => s.Name).ToListAsync(ct);
    public Task<Supplier?> GetByIdAsync(int id, int businessId, CancellationToken ct)
        => _db.Suppliers.FirstOrDefaultAsync(
            s => s.Id == id && s.BusinessId == businessId, ct);
    public Task<bool> EmailExistsAsync(
        string email, int businessId, int? exceptId, CancellationToken ct)
        => _db.Suppliers.AnyAsync(s => s.Email == email &&
            s.BusinessId == businessId && (!exceptId.HasValue || s.Id != exceptId), ct);
    public Task<bool> HasAnyProductsAsync(
        int id, int businessId, CancellationToken ct)
        => _db.Products.AnyAsync(
            p => p.SupplierId == id && p.BusinessId == businessId, ct);
    public Task<bool> HasActiveProductsAsync(
        int id, int businessId, CancellationToken ct)
        => _db.Products.AnyAsync(
            p => p.SupplierId == id && p.BusinessId == businessId && p.IsActive, ct);
    public async Task AddAsync(Supplier supplier, CancellationToken ct)
    {
        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync(ct);
    }
    public async Task UpdateAsync(Supplier supplier, CancellationToken ct)
    {
        _db.Suppliers.Update(supplier);
        await _db.SaveChangesAsync(ct);
    }
    public async Task DeleteAsync(Supplier supplier, CancellationToken ct)
    {
        _db.Suppliers.Remove(supplier);
        await _db.SaveChangesAsync(ct);
    }
}
