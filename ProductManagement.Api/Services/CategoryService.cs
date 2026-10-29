using ProductManagement.Api.DTOs;
using ProductManagement.Api.Exceptions;
using ProductManagement.Api.Models;
using ProductManagement.Api.Repositories;

namespace ProductManagement.Api.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBusinessRepository _businessRepository;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IBusinessRepository businessRepository)
    {
        _categoryRepository = categoryRepository;
        _businessRepository = businessRepository;
    }

    private static CategoryResponseDto Map(Category category)
        => new(category.Id, category.Name, category.Description,
            category.IsActive, category.CreatedAtUtc, category.UpdatedAtUtc);

    private async Task<int> GetBusinessIdAsync(
        string userId, CancellationToken ct)
    {
        var business = await _businessRepository.GetByOwnerIdAsync(userId, ct);
        return business?.Id ?? throw new BusinessNotAssociatedException();
    }

    public async Task<IReadOnlyCollection<CategoryResponseDto>> GetAllAsync(
        CategoryQuery query, string userId, CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);
        var result = await _categoryRepository.GetAllAsync(query, businessId, ct);
        return result.Items.Select(Map).ToArray();
    }

    public async Task<CategoryResponseDto?> GetByIdAsync(
        int id, string userId, CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);
        var category = await _categoryRepository.GetByIdAsync(id, businessId, ct);
        return category is null ? null : Map(category);
    }

    public async Task<CategoryResponseDto> CreateAsync(
        CreateCategoryDto request, string userId, CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Name is required.");

        var category = new Category
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            BusinessId = businessId
        };

        return Map(await _categoryRepository.AddAsync(category, ct));
    }

    public async Task<bool> UpdateAsync(
        int id, UpdateCategoryDto request, string userId, CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);
        var category = await _categoryRepository.GetByIdAsync(id, businessId, ct);
        if (category is null) return false;
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Name is required.");

        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim();
        category.IsActive = request.IsActive;
        category.UpdatedAtUtc = DateTime.UtcNow;
        await _categoryRepository.UpdateAsync(category, ct);
        return true;
    }

    public async Task<bool> DeleteAsync(
        int id, string userId, CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);
        var category = await _categoryRepository.GetByIdAsync(id, businessId, ct);
        if (category is null) return false;
        if (await _categoryRepository.IsUsedByProductsAsync(
                id, businessId, ct))
            throw new InvalidOperationException(
                "Remove product references before deleting this category.");

        await _categoryRepository.DeleteAsync(category, ct);
        return true;
    }
}
