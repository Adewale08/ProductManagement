using Microsoft.EntityFrameworkCore;
using ProductManagement.Api.Data;
using ProductManagement.Api.Models;

namespace ProductManagement.Api.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id,CancellationToken ct)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<List<Order>> GetAllAsync(
        CancellationToken ct)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ToListAsync(ct);
    }

    public async Task AddAsync(
        Order order,
        CancellationToken ct)
    {
        await _context.Orders.AddAsync(order, ct);
    }

    public async Task SaveChangesAsync(
        CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
}
