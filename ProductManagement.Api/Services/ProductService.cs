
using Microsoft.Extensions.Logging;
using ProductManagement.Api.DTOs;
using ProductManagement.Api.Exceptions;
using ProductManagement.Api.Repositories;
using ProductManagement.Api.Models;

namespace ProductManagement.Api.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IBusinessRepository _businessRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository repository,
        IBusinessRepository businessRepository,
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _businessRepository = businessRepository;
        _logger = logger;
    }

    private static string NormaliseSku(string sku)
        => sku.Trim().ToUpperInvariant();

    private static void EnsureValidBusinessValues(decimal price, int stock)
    {
        if (price < 0)
            throw new ValidationException("Price must be zero or greater.");

        if (stock < 0)
            throw new ValidationException("Stock quantity must be zero or greater.");
    }

    private async Task<int?> GetBusinessIdAsync(
        string userId,
        CancellationToken ct)
    {
        var business = await _businessRepository.GetByOwnerIdAsync(
            userId, ct);

        return business?.Id;
    }

    private static ProductResponseDto MapToResponse(Product product)
        => new(
            product.Id,
            product.Name,
            product.Description,
            product.Sku,
            product.Category,
            product.Price,
            product.StockQuantity,
            product.IsActive,
            product.CreatedAtUtc,
            product.UpdatedAtUtc);

    public async Task<IReadOnlyCollection<ProductResponseDto>> GetAllAsync(
        string userId,
        CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);

        if (businessId is null)
            return Array.Empty<ProductResponseDto>();

        var (items, _) = await _repository.GetAllAsync(
            new ProductQuery
            {
                Page = 1,
                PageSize = 100,
                SortBy = "name",
                SortDirection = "asc"
            },
            businessId.Value,
            ct);

        return items.Select(MapToResponse).ToList();
    }

    public async Task<ProductResponseDto?> GetByIdAsync(
        int id,
        string userId,
        CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);

        if (businessId is null)
            return null;

        var product = await _repository.GetByIdAsync(
            id, businessId.Value, ct);

        return product is null ? null : MapToResponse(product);
    }

    public async Task<ProductResponseDto> CreateAsync(
        CreateProductDto request,
        string userId,
        CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);

        if (businessId is null)
        {
            throw new BusinessNotAssociatedException();
        }

        var normalisedSku = NormaliseSku(request.Sku);

        var existingProduct = await _repository.GetBySkuAsync(
            normalisedSku, businessId.Value, ct);

        if (existingProduct is not null)
            throw new DuplicateSkuException(request.Sku);

        EnsureValidBusinessValues(request.Price, request.StockQuantity);

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Sku = normalisedSku,
            Category = request.Category,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,

            // Assign ownership on the server.
            BusinessId = businessId.Value
        };

        _logger.LogInformation(
            "Creating product with SKU {Sku} for business {BusinessId}",
            normalisedSku,
            businessId.Value);

        await _repository.AddAsync(product, ct);

        return MapToResponse(product);
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateProductDto request,
        string userId,
        CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);

        if (businessId is null)
            return false;

        var product = await _repository.GetByIdAsync(
            id, businessId.Value, ct);

        if (product is null)
            return false;

        EnsureValidBusinessValues(request.Price, request.StockQuantity);

        var newSku = NormaliseSku(request.Sku);

        if (!string.Equals(
            product.Sku,
            newSku,
            StringComparison.OrdinalIgnoreCase))
        {
            var existingProduct = await _repository.GetBySkuAsync(
                newSku, businessId.Value, ct);

            if (existingProduct is not null &&
                existingProduct.Id != id)
            {
                throw new DuplicateSkuException(request.Sku);
            }
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Sku = newSku;
        product.Category = request.Category;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.IsActive = request.IsActive;
        product.UpdatedAtUtc = DateTime.UtcNow;

        await _repository.UpdateAsync(product, ct);

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        string userId,
        CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);

        if (businessId is null)
            return false;

        var product = await _repository.GetByIdAsync(
            id, businessId.Value, ct);

        if (product is null)
            return false;

        await _repository.DeleteAsync(product, ct);

        return true;
    }

    public async Task<bool> UpdateStockAsync(
        int id,
        int stockQuantity,
        string userId,
        CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);

        if (businessId is null)
            return false;

        var product = await _repository.GetByIdAsync(
            id, businessId.Value, ct);

        if (product is null)
            return false;

        EnsureValidBusinessValues(product.Price, stockQuantity);

        product.StockQuantity = stockQuantity;
        product.UpdatedAtUtc = DateTime.UtcNow;

        await _repository.UpdateAsync(product, ct);

        return true;
    }
}
