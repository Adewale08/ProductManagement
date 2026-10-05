using Microsoft.AspNetCore.Mvc;
using ProductManagement.Api.DTOs;
using ProductManagement.Api.Services;

namespace ProductManagement.Api.Controllers;

[ApiController]
[Route("[controller]")]

public class CategoryController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoryController(ICategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CategoryResponseDto>>> GetAll([FromQuery] CategoryQuery query, CancellationToken ct)
    {
        var categories = await _service.GetAllAsync(query, ct);

        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponseDto>> GetById(int id, CancellationToken ct)
    {
        var category = await _service.GetByIdAsync(id, ct);

        return category is null ? NotFound() : Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> Create(CreateCategoryDto request, CancellationToken ct)
    {
        var category = await _service.CreateAsync(request, ct);
        return Created($"/Category/{category.Id}", category);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateCategoryDto request,
        CancellationToken ct)
    {
        var updated = await _service.UpdateAsync(id, request, ct);

        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken ct)
    {
        var deleted = await _service.DeleteAsync(id, ct);

        return deleted ? NoContent() : NotFound();
    }
}    