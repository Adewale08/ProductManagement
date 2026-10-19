namespace ProductManagement.Api.DTOs;

public record SupplierResponseDto(
    int Id,
    string Name,
    string Email,
    string? PhoneNumber,
    bool IsActive);