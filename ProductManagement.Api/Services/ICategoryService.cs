using ProductManagement.Api.DTOs;
namespace ProductManagement.Api.Services;

public interface ICategoryService
{
    Task<IReadOnlyCollection<CategoryResponseDto>> GetAllAsync(CategoryQuery query, CancellationToken ct);
    Task<CategoryResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CategoryResponseDto> CreateAsync(CreateCategoryDto request, CancellationToken ct);
    Task<bool> UpdateAsync(int id, UpdateCategoryDto request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}