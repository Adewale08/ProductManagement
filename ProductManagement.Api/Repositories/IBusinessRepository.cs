using ProductManagement.Api.Models;

namespace ProductManagement.Api.Repositories;

public interface IBusinessRepository
{
    Task<Business?> GetByOwnerIdAsync(
        string ownerId,
        CancellationToken ct);
}
