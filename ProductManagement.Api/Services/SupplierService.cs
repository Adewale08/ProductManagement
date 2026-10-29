using ProductManagement.Api.DTOs;
using ProductManagement.Api.Exceptions;
using ProductManagement.Api.Models;
using ProductManagement.Api.Repositories;

namespace ProductManagement.Api.Services;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _repository;
    private readonly IBusinessRepository _businessRepository;

    public SupplierService(
        ISupplierRepository repository,
        IBusinessRepository businessRepository)
    {
        _repository = repository;
        _businessRepository = businessRepository;
    }

    private static SupplierResponseDto Map(Supplier supplier)
        => new(supplier.Id, supplier.Name, supplier.Email,
            supplier.PhoneNumber, supplier.IsActive);

    private async Task<int> GetBusinessIdAsync(
        string userId, CancellationToken ct)
    {
        var business = await _businessRepository.GetByOwnerIdAsync(userId, ct);
        return business?.Id ?? throw new BusinessNotAssociatedException();
    }

    public async Task<List<SupplierResponseDto>> GetAllAsync(
        string userId, CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);
        var rows = await _repository.GetAllAsync(businessId, ct);
        return rows.Select(Map).ToList();
    }

    public async Task<SupplierResponseDto?> GetByIdAsync(
        int id, string userId, CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);
        var row = await _repository.GetByIdAsync(id, businessId, ct);
        return row is null ? null : Map(row);
    }

    public async Task<SupplierResponseDto> CreateAsync(
        CreateSupplierDto request, string userId, CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _repository.EmailExistsAsync(email, businessId, null, ct))
            throw new InvalidOperationException(
                "A supplier with this email already exists in this business.");

        var supplier = new Supplier
        {
            Name = request.Name.Trim(),
            Email = email,
            PhoneNumber = request.PhoneNumber?.Trim(),
            IsActive = true,
            BusinessId = businessId
        };
        await _repository.AddAsync(supplier, ct);
        return Map(supplier);
    }

    public async Task<bool> UpdateAsync(
        int id, UpdateSupplierDto request, string userId, CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);
        var supplier = await _repository.GetByIdAsync(id, businessId, ct);
        if (supplier is null) return false;

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _repository.EmailExistsAsync(email, businessId, id, ct))
            throw new InvalidOperationException(
                "A supplier with this email already exists in this business.");

        supplier.Name = request.Name.Trim();
        supplier.Email = email;
        supplier.PhoneNumber = request.PhoneNumber?.Trim();
        supplier.IsActive = request.IsActive;
        await _repository.UpdateAsync(supplier, ct);
        return true;
    }

    public async Task<bool> DeleteAsync(
        int id, string userId, CancellationToken ct)
    {
        var businessId = await GetBusinessIdAsync(userId, ct);
        var supplier = await _repository.GetByIdAsync(id, businessId, ct);
        if (supplier is null) return false;
        if (await _repository.HasActiveProductsAsync(id, businessId, ct))
            throw new InvalidOperationException("Supplier has active products.");
        if (await _repository.HasAnyProductsAsync(id, businessId, ct))
            throw new InvalidOperationException(
                "Remove product references before deleting supplier.");

        await _repository.DeleteAsync(supplier, ct);
        return true;
    }
}
