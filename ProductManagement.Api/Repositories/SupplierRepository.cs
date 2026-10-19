using Microsoft.EntityFrameworkCore;
using ProductManagement.Api.Data;
using ProductManagement.Api.Models;
namespace ProductManagement.Api.Repositories;
public class SupplierRepository : ISupplierRepository
{
    private readonly AppDbContext _db;
    public SupplierRepository(AppDbContext db) => _db = db;
    public Task<List<Supplier>> GetAllAsync(CancellationToken ct)
        => _db.Suppliers.AsNoTracking()
            .OrderBy(s => s.Name).ToListAsync(ct);
    public Task<Supplier?> GetByIdAsync(int id, CancellationToken ct)
        => _db.Suppliers.FirstOrDefaultAsync(s => s.Id == id, ct);
    public Task<bool> EmailExistsAsync(string email, CancellationToken ct)
        => _db.Suppliers.AnyAsync(s => s.Email == email, ct);
    public Task<bool> HasAnyProductsAsync(int id, CancellationToken ct)
        => _db.Products.AnyAsync(p => p.SupplierId == id, ct);
    public Task<bool> HasActiveProductsAsync(int id, CancellationToken ct)
        => _db.Products.AnyAsync(
            p => p.SupplierId == id && p.IsActive, ct);
    public async Task AddAsync(Supplier supplier, CancellationToken ct)
    {
        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync(ct);
    }
    public async Task DeleteAsync(Supplier supplier, CancellationToken ct)
    {
        _db.Suppliers.Remove(supplier);
        await _db.SaveChangesAsync(ct);
    }
}