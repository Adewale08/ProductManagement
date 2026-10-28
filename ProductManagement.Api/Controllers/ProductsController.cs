using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Api.DTOs;
using ProductManagement.Api.Services;

namespace ProductManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProductResponseDto>>> GetAll(
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized();

        var products = await _service.GetAllAsync(userId, ct);

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> GetById(
        int id,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized();

        var product = await _service.GetByIdAsync(id, userId, ct);

        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> Create(
        CreateProductDto request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized();

        var product = await _service.CreateAsync(request, userId, ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateProductDto request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized();

        var updated = await _service.UpdateAsync(
            id, request, userId, ct);

        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized();

        var deleted = await _service.DeleteAsync(id, userId, ct);

        return deleted ? NoContent() : NotFound();
    }

    [HttpPatch("{id:int}/stock")]
    public async Task<IActionResult> UpdateStock(
        int id,
        UpdateStockDto request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized();

        var updated = await _service.UpdateStockAsync(
            id, request.StockQuantity, userId, ct);

        return updated ? NoContent() : NotFound();
    }
}
