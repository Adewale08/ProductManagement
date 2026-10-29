using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Api.DTOs;
using ProductManagement.Api.Services;

namespace ProductManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SuppliersController(ISupplierService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SupplierResponseDto>>> GetAll(
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        return userId is null
            ? Unauthorized()
            : Ok(await service.GetAllAsync(userId, ct));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SupplierResponseDto>> GetById(
        int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();
        var supplier = await service.GetByIdAsync(id, userId, ct);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [HttpPost]
    public async Task<ActionResult<SupplierResponseDto>> Create(
        CreateSupplierDto request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();
        try
        {
            var created = await service.CreateAsync(request, userId, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, UpdateSupplierDto request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();
        try
        {
            var updated = await service.UpdateAsync(id, request, userId, ct);
            return updated ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();
        try
        {
            var deleted = await service.DeleteAsync(id, userId, ct);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    private string? GetCurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier);
}
