using ProductManagement.Api.DTOs;
using ProductManagement.Api.Results;

namespace ProductManagement.Api.Services;

public interface IAuthService
{
    Task<AuthServiceResult> RegisterAsync(RegisterDto request);
    Task<AuthServiceResult> LoginAsync(LoginDto request);
    Task LogoutAsync();
}
