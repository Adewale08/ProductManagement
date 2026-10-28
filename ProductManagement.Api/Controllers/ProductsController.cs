using Microsoft.AspNetCore.Mvc;
using ProductManagement.Api.DTOs;
using ProductManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;

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

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProductResponseDto>>> GetAll(CancellationToken ct)
    {
        var products = await _service.GetAllAsync(ct);

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> GetById(int id, CancellationToken ct)
    {
        var product = await _service.GetByIdAsync(id, ct);

        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> Create(CreateProductDto request, CancellationToken ct)
    {
        var product = await _service.CreateAsync(request, ct);

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

    [HttpPatch("{id:int}/stock")]
    public async Task<IActionResult> UpdateStock(
        int id,
        UpdateStockDto request,
        CancellationToken ct)
    {
        var updated = await _service.UpdateStockAsync(
            id,
            request.StockQuantity,
            ct);

        return updated ? NoContent() : NotFound();
    }
}