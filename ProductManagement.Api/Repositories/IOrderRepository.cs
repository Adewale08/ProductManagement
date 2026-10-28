using ProductManagement.Api.Models;

namespace ProductManagement.Api.Repositories;

public interface IOrderRepository
{
    Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken ct);

    Task<IReadOnlyCollection<Product>> GetProductsByIdsAsync(
        IReadOnlyCollection<int> productIds,
        int businessId,
        CancellationToken ct);

    Task<bool> TryDecreaseStockAsync(
        int productId,
        int businessId,
        int quantity,
        decimal expectedPrice,
        CancellationToken ct);

    Task<bool> TryRestoreStockAsync(
        int productId,
        int businessId,
        int quantity,
        CancellationToken ct);

    Task<Order?> GetByIdAsync(int id, int businessId, CancellationToken ct);

    Task<IReadOnlyCollection<Order>> GetAllAsync(
        int businessId,
        CancellationToken ct);

    Task<bool> TryTransitionStatusAsync(
        int id,
        int businessId,
        string currentStatus,
        string newStatus,
        CancellationToken ct);

    Task AddAsync(Order order, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
