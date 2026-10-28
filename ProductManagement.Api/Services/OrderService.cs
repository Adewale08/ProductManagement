using ProductManagement.Api.DTOs;
using ProductManagement.Api.Exceptions;
using ProductManagement.Api.Models;
using ProductManagement.Api.Repositories;

namespace ProductManagement.Api.Services;

public class OrderService(
    IOrderRepository orderRepository,
    IBusinessRepository businessRepository) : IOrderService
{
    private const string PendingStatus = "Pending";
    private const string CompletedStatus = "Completed";
    private const string CancelledStatus = "Cancelled";
    private const decimal MaximumSqlDecimal18_2 = 9999999999999999.99m;

    public async Task<OrderResponseDto> CreateAsync(
        CreateOrderDto request,
        string userId,
        CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);
        var quantities = AggregateItems(request);
        var productIds = quantities.Keys.Order().ToArray();

        return await orderRepository.ExecuteInTransactionAsync(
            async transactionCt =>
            {
                var products = await orderRepository.GetProductsByIdsAsync(
                    productIds,
                    businessId,
                    transactionCt);
                var productsById = products.ToDictionary(product => product.Id);

                if (productsById.Count != productIds.Length)
                {
                    throw new ValidationException(
                        "One or more products are unavailable for this business.");
                }

                foreach (var productId in productIds)
                {
                    var product = productsById[productId];
                    var quantity = quantities[productId];

                    if (!product.IsActive)
                    {
                        throw new OrderConflictException(
                            $"Product '{product.Name}' is not active.");
                    }

                    if (product.Price < 0)
                    {
                        throw new ValidationException(
                            $"Product '{product.Name}' has an invalid price.");
                    }

                    if (product.StockQuantity < quantity)
                    {
                        throw new OrderConflictException(
                            $"Insufficient stock for product '{product.Name}'.");
                    }
                }

                var items = new List<OrderItem>(productIds.Length);
                decimal totalAmount = 0;

                foreach (var productId in productIds)
                {
                    var product = productsById[productId];
                    var quantity = quantities[productId];
                    var lineTotal = product.Price * quantity;

                    if (lineTotal > MaximumSqlDecimal18_2 ||
                        totalAmount > MaximumSqlDecimal18_2 - lineTotal)
                    {
                        throw new ValidationException(
                            "The order total exceeds the supported amount.");
                    }

                    totalAmount += lineTotal;
                    items.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = quantity,
                        UnitPrice = product.Price
                    });
                }

                foreach (var productId in productIds)
                {
                    var product = productsById[productId];
                    var stockDecreased =
                        await orderRepository.TryDecreaseStockAsync(
                            productId,
                            businessId,
                            quantities[productId],
                            product.Price,
                            transactionCt);

                    if (!stockDecreased)
                    {
                        throw new OrderConflictException(
                            "Product stock or price changed while placing the order. Please retry.");
                    }
                }

                var order = new Order
                {
                    BusinessId = businessId,
                    CreatedAtUtc = DateTime.UtcNow,
                    Status = PendingStatus,
                    TotalAmount = totalAmount,
                    Items = items
                };

                await orderRepository.AddAsync(order, transactionCt);
                await orderRepository.SaveChangesAsync(transactionCt);

                return MapToResponse(order, productsById);
            },
            ct);
    }

    public async Task<IReadOnlyCollection<OrderResponseDto>> GetAllAsync(
        string userId,
        CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);
        var orders = await orderRepository.GetAllAsync(businessId, ct);

        return orders.Select(order => MapToResponse(order)).ToArray();
    }

    public async Task<OrderResponseDto?> GetByIdAsync(
        int id,
        string userId,
        CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);
        var order = await orderRepository.GetByIdAsync(id, businessId, ct);

        return order is null ? null : MapToResponse(order);
    }

    public async Task<OrderResponseDto> UpdateStatusAsync(
        int id,
        UpdateOrderStatusDto request,
        string userId,
        CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);
        var targetStatus = request.Status.Trim().ToLowerInvariant() switch
        {
            "completed" => CompletedStatus,
            "cancelled" => CancelledStatus,
            _ => throw new ValidationException(
                "Status must be either 'Completed' or 'Cancelled'.")
        };

        return await orderRepository.ExecuteInTransactionAsync(
            async transactionCt =>
            {
                var existingOrder = await orderRepository.GetByIdAsync(
                    id,
                    businessId,
                    transactionCt);

                if (existingOrder is null)
                {
                    throw new OrderNotFoundException(id);
                }

                if (existingOrder.Status != PendingStatus)
                {
                    throw new OrderConflictException(
                        "Only pending orders can be completed or cancelled.");
                }

                var transitioned =
                    await orderRepository.TryTransitionStatusAsync(
                        id,
                        businessId,
                        PendingStatus,
                        targetStatus,
                        transactionCt);

                if (!transitioned)
                {
                    throw new OrderConflictException(
                        "The order status changed while processing the request.");
                }

                if (targetStatus == CancelledStatus)
                {
                    foreach (var item in existingOrder.Items)
                    {
                        var stockRestored =
                            await orderRepository.TryRestoreStockAsync(
                                item.ProductId,
                                businessId,
                                item.Quantity,
                                transactionCt);

                        if (!stockRestored)
                        {
                            throw new OrderConflictException(
                                "Inventory could not be restored for the cancelled order.");
                        }
                    }
                }

                var updatedOrder = await orderRepository.GetByIdAsync(
                    id,
                    businessId,
                    transactionCt);

                if (updatedOrder is null)
                {
                    throw new OrderConflictException(
                        "The order could not be loaded after its status changed.");
                }

                return MapToResponse(updatedOrder);
            },
            ct);
    }

    private async Task<int> GetBusinessIdAsync(
        string userId,
        CancellationToken ct)
    {
        var business = await businessRepository.GetByOwnerIdAsync(userId, ct);

        if (business is null)
        {
            throw new BusinessNotAssociatedException();
        }

        return business.Id;
    }

    private static Dictionary<int, int> AggregateItems(CreateOrderDto request)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ValidationException(
                "An order must contain at least one item.");
        }

        var quantities = new Dictionary<int, int>();

        foreach (var item in request.Items)
        {
            if (item.ProductId < 1)
            {
                throw new ValidationException(
                    "Product IDs must be greater than zero.");
            }

            if (item.Quantity < 1)
            {
                throw new ValidationException(
                    "Item quantities must be at least one.");
            }

            try
            {
                quantities[item.ProductId] = checked(
                    quantities.GetValueOrDefault(item.ProductId) + item.Quantity);
            }
            catch (OverflowException)
            {
                throw new ValidationException(
                    "The combined quantity for a product is too large.");
            }
        }

        return quantities;
    }

    private static OrderResponseDto MapToResponse(
        Order order,
        IReadOnlyDictionary<int, Product>? productLookup = null)
    {
        var items = order.Items
            .Select(item =>
            {
                var productName = productLookup is null
                    ? item.Product.Name
                    : productLookup[item.ProductId].Name;

                return new OrderItemResponseDto(
                    item.ProductId,
                    productName,
                    item.Quantity,
                    item.UnitPrice,
                    item.UnitPrice * item.Quantity);
            })
            .ToArray();

        return new OrderResponseDto(
            order.Id,
            order.CreatedAtUtc,
            order.Status,
            order.TotalAmount,
            items);
    }
}
