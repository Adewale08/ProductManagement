
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Api.Data;
using ProductManagement.Api.DTOs;
using ProductManagement.Api.Models;
using ProductManagement.Api.Results;

namespace ProductManagement.Api.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly AppDbContext _dbContext;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        AppDbContext dbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
    }

    public async Task<AuthServiceResult> RegisterAsync(RegisterDto request)
    {
        var businessName = request.BusinessName?.Trim();

        if (string.IsNullOrWhiteSpace(businessName))
        {
            return new AuthServiceResult
            {
                Succeeded = false,
                Message = "Business name is required."
            };
        }

        var existingUser =
            await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return new AuthServiceResult
            {
                Succeeded = false,
                Message = "An account with this email already exists."
            };
        }

        var existingBusiness = await _dbContext.Businesses
            .AnyAsync(business => business.Name == businessName);

        if (existingBusiness)
        {
            return new AuthServiceResult
            {
                Succeeded = false,
                Message = "This business name is already taken."
            };
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var user = new ApplicationUser
            {
                UserName = request.BusinessName,
                Email = request.Email
            };

            var createUserResult = await _userManager.CreateAsync(
                user,
                request.Password);

            if (!createUserResult.Succeeded)
            {
                await transaction.RollbackAsync();

                return new AuthServiceResult
                {
                    Succeeded = false,
                    Message = "Account creation failed.",
                    Errors = createUserResult.Errors
                        .Select(error => error.Description)
                        .ToArray()
                };
            }

            var business = new Business
            {
                Name = businessName,
                OwnerId = user.Id
            };

            _dbContext.Businesses.Add(business);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return new AuthServiceResult
            {
                Succeeded = true,
                Message = "Account and business created successfully."
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<AuthServiceResult> LoginAsync(LoginDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null || string.IsNullOrEmpty(user.UserName))
        {
            return new AuthServiceResult
            {
                Succeeded = false,
                Message = "Invalid email or password."
            };
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName,
            request.Password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return new AuthServiceResult
            {
                Succeeded = false,
                Message = "Invalid email or password."
            };
        }

        return new AuthServiceResult
        {
            Succeeded = true,
            Message = "Login successful."
        };
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
