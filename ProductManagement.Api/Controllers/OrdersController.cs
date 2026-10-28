using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Api.DTOs;
using ProductManagement.Api.Services;

namespace ProductManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<OrderResponseDto>> Create(
        CreateOrderDto request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var order = await orderService.CreateAsync(request, userId, ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = order.Id },
            order);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<OrderResponseDto>>> GetAll(
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var orders = await orderService.GetAllAsync(userId, ct);
        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponseDto>> GetById(
        int id,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var order = await orderService.GetByIdAsync(id, userId, ct);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<OrderResponseDto>> UpdateStatus(
        int id,
        UpdateOrderStatusDto request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var order = await orderService.UpdateStatusAsync(
            id,
            request,
            userId,
            ct);

        return Ok(order);
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
