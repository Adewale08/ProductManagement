using Microsoft.EntityFrameworkCore;
using ProductManagement.Api.Data;
using ProductManagement.Api.Models;

namespace ProductManagement.Api.Repositories;

public class BusinessRepository : IBusinessRepository
{
    private readonly AppDbContext _db;

    public BusinessRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Business?> GetByOwnerIdAsync(
        string ownerId,
        CancellationToken ct)
    {
        return _db.Businesses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                business => business.OwnerId == ownerId,
                ct);
    }
}
