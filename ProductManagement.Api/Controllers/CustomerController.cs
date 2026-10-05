using Microsoft.AspNetCore.Mvc;
using ProductManagement.Api.DTOs;
using ProductManagement.Api.Services;

namespace ProductManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CustomerResponseDto>>> GetAllAsync([FromQuery] CustomerQuery request, CancellationToken ct)
    {
        var result = await _customerService.GetAllAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerResponseDto?>> GetByIdAsync(int id, CancellationToken ct)
    {
        var result = await _customerService.GetByIdAsync(id, ct);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponseDto>> CreateAsync(CreateCustomerDTO request, CancellationToken ct)
    {
        var result = await _customerService.CreateAsync(request, ct);
        return Created($"/api/Customer/{result.Id}", result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<bool>> UpdateAsync(int id, UpdateCustomerDTO request, CancellationToken ct)
    {
        var result = await _customerService.UpdateAsync(id, request, ct);
        if (!result)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync(int id, CancellationToken ct)
    {
        var result = await _customerService.DeleteAsync(id, ct);
        if (!result)
        {
            return NotFound();
        }
        return Ok(result);
    }
}