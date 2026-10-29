using ProductManagement.Api.DTOs;
namespace ProductManagement.Api.Services;

public interface ICategoryService
{
    Task<IReadOnlyCollection<CategoryResponseDto>> GetAllAsync(CategoryQuery query, string userId, CancellationToken ct);
    Task<CategoryResponseDto?> GetByIdAsync(int id, string userId, CancellationToken ct);
    Task<CategoryResponseDto> CreateAsync(CreateCategoryDto request, string userId, CancellationToken ct);
    Task<bool> UpdateAsync(int id, UpdateCategoryDto request, string userId, CancellationToken ct);
    Task<bool> DeleteAsync(int id, string userId, CancellationToken ct);
}
