
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
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository repository,
        IBusinessRepository businessRepository,
        ICategoryRepository categoryRepository,
        ISupplierRepository supplierRepository,
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _businessRepository = businessRepository;
        _categoryRepository = categoryRepository;
        _supplierRepository = supplierRepository;
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
            product.CategoryRecord?.Name ?? product.Category,
            product.Price,
            product.StockQuantity,
            product.IsActive,
            product.CreatedAtUtc,
            product.UpdatedAtUtc,
            product.CategoryId,
            product.SupplierId);

    private async Task<string> ValidateAndResolveCategoryAsync(
        string categoryName,
        int? categoryId,
        int businessId,
        CancellationToken ct)
    {
        if (!categoryId.HasValue) return categoryName;

        var category = await _categoryRepository.GetByIdAsync(
            categoryId.Value, businessId, ct);
        if (category is null)
            throw new ValidationException(
                "The selected category is unavailable for this business.");
        if (category.Name.Length > 80)
            throw new ValidationException(
                "A category assigned to a product must have a name of at most 80 characters.");

        return category.Name;
    }

    private async Task ValidateSupplierAsync(
        int? supplierId, int businessId, CancellationToken ct)
    {
        if (!supplierId.HasValue) return;
        var supplier = await _supplierRepository.GetByIdAsync(
            supplierId.Value, businessId, ct);
        if (supplier is null)
            throw new ValidationException(
                "The selected supplier is unavailable for this business.");
    }

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
        var categoryName = await ValidateAndResolveCategoryAsync(
            request.Category, request.CategoryId, businessId.Value, ct);
        await ValidateSupplierAsync(request.SupplierId, businessId.Value, ct);

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Sku = normalisedSku,
            Category = categoryName,
            CategoryId = request.CategoryId,
            SupplierId = request.SupplierId,
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
        var categoryId = request.CategoryId;
        if (!categoryId.HasValue && product.CategoryId.HasValue &&
            string.Equals(
                request.Category,
                product.CategoryRecord?.Name ?? product.Category,
                StringComparison.OrdinalIgnoreCase))
        {
            // Older clients only send the Category string. Keep an existing
            // relation when that string still represents the same category.
            categoryId = product.CategoryId;
        }
        var supplierId = request.SupplierId ?? product.SupplierId;
        var categoryName = await ValidateAndResolveCategoryAsync(
            request.Category, categoryId, businessId.Value, ct);
        await ValidateSupplierAsync(supplierId, businessId.Value, ct);

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
        product.Category = categoryName;
        product.CategoryId = categoryId;
        product.SupplierId = supplierId;
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
