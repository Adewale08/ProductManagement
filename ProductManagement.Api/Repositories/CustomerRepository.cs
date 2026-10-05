using Microsoft.EntityFrameworkCore;
using ProductManagement.Api.Data;
using ProductManagement.Api.DTOs;
using ProductManagement.Api.Models;

namespace ProductManagement.Api.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _db;

    public CustomerRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(IReadOnlyCollection<Customer> Items, int TotalCount)> GetAllAsync(CustomerQuery request, CancellationToken ct)
    {
        var query = _db.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim();
            query = query.Where(c => c.Name.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.Trim();
            query = query.Where(c => c.Email.Contains(email));
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var phone = request.PhoneNumber.Trim();
            query = query.Where(c => c.PhoneNumber.Contains(phone));
        }

        if (request.CreatedAtUtc.HasValue)
        {
            var createdDate = request.CreatedAtUtc.Value.Date;
            query = query.Where(c => c.CreatedAtUtc.Date == createdDate);
        }

        var totalCount = await query.CountAsync(ct);
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var items = await query
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public Task<Customer?> GetByIdAsync(int id, CancellationToken ct)
        => _db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AddAsync(Customer customer, CancellationToken ct)
    {
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken ct)
    {
        _db.Customers.Update(customer);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Customer customer, CancellationToken ct)
    {
        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync(ct);
    }
}