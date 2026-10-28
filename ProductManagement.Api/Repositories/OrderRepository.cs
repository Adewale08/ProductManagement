using Microsoft.EntityFrameworkCore;
using ProductManagement.Api.Data;
using ProductManagement.Api.Models;

namespace ProductManagement.Api.Repositories;

public class OrderRepository(AppDbContext context) : IOrderRepository
{
    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken ct)
    {
        await using var transaction =
            await context.Database.BeginTransactionAsync(ct);

        var result = await operation(ct);
        await transaction.CommitAsync(ct);

        return result;
    }

    public async Task<IReadOnlyCollection<Product>> GetProductsByIdsAsync(
        IReadOnlyCollection<int> productIds,
        int businessId,
        CancellationToken ct)
    {
        return await context.Products
            .AsNoTracking()
            .Where(product =>
                productIds.Contains(product.Id) &&
                product.BusinessId == businessId)
            .ToListAsync(ct);
    }

    public async Task<bool> TryDecreaseStockAsync(
        int productId,
        int businessId,
        int quantity,
        decimal expectedPrice,
        CancellationToken ct)
    {
        var affectedRows = await context.Products
            .Where(product =>
                product.Id == productId &&
                product.BusinessId == businessId &&
                product.IsActive &&
                product.Price == expectedPrice &&
                product.StockQuantity >= quantity)
            .ExecuteUpdateAsync(updates => updates
                .SetProperty(
                    product => product.StockQuantity,
                    product => product.StockQuantity - quantity)
                .SetProperty(
                    product => product.UpdatedAtUtc,
                    DateTime.UtcNow),
                ct);

        return affectedRows == 1;
    }

    public async Task<bool> TryRestoreStockAsync(
        int productId,
        int businessId,
        int quantity,
        CancellationToken ct)
    {
        var affectedRows = await context.Products
            .Where(product =>
                product.Id == productId &&
                product.BusinessId == businessId)
            .ExecuteUpdateAsync(updates => updates
                .SetProperty(
                    product => product.StockQuantity,
                    product => product.StockQuantity + quantity)
                .SetProperty(
                    product => product.UpdatedAtUtc,
                    DateTime.UtcNow),
                ct);

        return affectedRows == 1;
    }

    public Task<Order?> GetByIdAsync(
        int id,
        int businessId,
        CancellationToken ct)
    {
        return context.Orders
            .AsNoTracking()
            .Where(order =>
                order.Id == id &&
                order.BusinessId == businessId)
            .Include(order => order.Items)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyCollection<Order>> GetAllAsync(
        int businessId,
        CancellationToken ct)
    {
        return await context.Orders
            .AsNoTracking()
            .Where(order => order.BusinessId == businessId)
            .Include(order => order.Items)
                .ThenInclude(item => item.Product)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<bool> TryTransitionStatusAsync(
        int id,
        int businessId,
        string currentStatus,
        string newStatus,
        CancellationToken ct)
    {
        var affectedRows = await context.Orders
            .Where(order =>
                order.Id == id &&
                order.BusinessId == businessId &&
                order.Status == currentStatus)
            .ExecuteUpdateAsync(updates => updates
                .SetProperty(order => order.Status, newStatus),
                ct);

        return affectedRows == 1;
    }

    public Task AddAsync(Order order, CancellationToken ct)
    {
        return context.Orders.AddAsync(order, ct).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return context.SaveChangesAsync(ct);
    }
}
