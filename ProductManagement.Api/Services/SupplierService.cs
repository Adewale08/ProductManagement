using ProductManagement.Api.DTOs;
using ProductManagement.Api.Models;
using ProductManagement.Api.Repositories;

namespace ProductManagement.Api.Services;

public class SupplierService : ISupplierService{
    private readonly ISupplierRepository _repository;
    public SupplierService(ISupplierRepository repository)
        => _repository = repository;
    private static SupplierResponseDto Map(Supplier s)
        => new(s.Id, s.Name, s.Email, s.PhoneNumber, s.IsActive);
    public async Task<List<SupplierResponseDto>> GetAllAsync(CancellationToken ct)
    {
        var rows = await _repository.GetAllAsync(ct);
        return rows.Select(Map).ToList();
    }
    public async Task<SupplierResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var row = await _repository.GetByIdAsync(id, ct);
        return row is null ? null : Map(row);
    }
    public async Task<SupplierResponseDto> CreateAsync(CreateSupplierDto request, CancellationToken ct)
    {
        string email = request.Email.Trim().ToLowerInvariant();
        if (await _repository.EmailExistsAsync(email, ct))
            throw new InvalidOperationException(
                "A supplier with this email already exists.");
        var supplier = new Supplier
        {
            Name = request.Name.Trim(),
            Email = email,
            PhoneNumber = request.PhoneNumber?.Trim(),
            IsActive = true
        };
        await _repository.AddAsync(supplier, ct);
        return Map(supplier);
    }
    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var supplier = await _repository.GetByIdAsync(id, ct);
        if (supplier is null) return false;
        if (await _repository.HasActiveProductsAsync(id, ct))
            throw new InvalidOperationException(
                "Supplier has active products.");
        if (await _repository.HasAnyProductsAsync(id, ct))
            throw new InvalidOperationException(
                "Remove product references before deleting supplier.");
        await _repository.DeleteAsync(supplier, ct);
        return true;
    }
}