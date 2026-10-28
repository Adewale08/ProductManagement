using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Api.DTOs;
using ProductManagement.Api.Results;
using ProductManagement.Api.Services;

namespace ProductManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        var result = await _authService.RegisterAsync(request);

        if (result.Succeeded)
        {
            return StatusCode(StatusCodes.Status201Created, new
            {
                message = result.Message
            });
        }

        return result.FailureReason switch
        {
            AuthFailureReason.Conflict =>
                Conflict(new { message = result.Message }),

            AuthFailureReason.Validation =>
                BadRequest(new
                {
                    message = result.Message,
                    errors = result.Errors
                }),

            _ => BadRequest(new
            {
                message = result.Message,
                errors = result.Errors
            })
        };
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        var result = await _authService.LoginAsync(request);

        if (result.Succeeded)
        {
            return Ok(new { message = result.Message });
        }

        return Unauthorized(new { message = result.Message });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            businessName = User.Identity?.Name,
            email = User.FindFirstValue(ClaimTypes.Email)
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();

        return Ok(new { message = "Logged out successfully." });
    }
}
