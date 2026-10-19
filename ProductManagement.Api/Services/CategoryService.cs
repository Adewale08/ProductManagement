using ProductManagement.Api.DTOs;
using Microsoft.Extensions.Logging;
using ProductManagement.Api.Exceptions;
using ProductManagement.Api.Repositories;
using ProductManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ProductManagement.Api.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<CategoryResponseDto>> GetAllAsync(CategoryQuery query, CancellationToken ct)
    {
        var result = await _categoryRepository.GetAllAsync(query, ct);

        return result.Items
            .Select(c => new CategoryResponseDto(
                c.Id,
                c.Name,
                c.Description,
                c.IsActive,
                c.CreatedAtUtc,
                c.UpdatedAtUtc))
            .ToList();
    }

    public async Task<CategoryResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var category = await _categoryRepository.GetByIdAsync(id, ct);
        if (category == null) return null;

        return new CategoryResponseDto(
            category.Id,
            category.Name,
            category.Description,
            category.IsActive,
            category.CreatedAtUtc,
            category.UpdatedAtUtc);
    }

    private static CategoryResponseDto MapToResponse(Category category)
    {
        return new CategoryResponseDto(
            category.Id,
            category.Name,
            category.Description,
            category.IsActive,
            category.CreatedAtUtc,
            category.UpdatedAtUtc
        );
    }
    public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Name is required.");
        }
        var newCategory = new Category
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var createdCategory = await _categoryRepository.AddAsync(newCategory, ct);
        return MapToResponse(createdCategory);

    }
    public async Task<bool> UpdateAsync(int id, UpdateCategoryDto request, CancellationToken ct)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(id, ct);
        if (existingCategory == null) return false;

        existingCategory.Name = request.Name;
        existingCategory.Description = request.Description;
        existingCategory.IsActive = request.IsActive;
        existingCategory.UpdatedAtUtc = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(existingCategory, ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(id, ct);
        if (existingCategory == null) return false;

        await _categoryRepository.DeleteAsync(existingCategory, ct);
        return true;
    }
}    

