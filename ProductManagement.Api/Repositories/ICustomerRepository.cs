using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProductManagement.Api.Models;
using ProductManagement.Api.DTOs;

namespace ProductManagement.Api.Repositories;

public interface ICustomerRepository
{
    Task<(IReadOnlyCollection<Customer> Items, int TotalCount)> GetAllAsync(CustomerQuery request, CancellationToken ct);
    Task<Customer?> GetByIdAsync(int id, CancellationToken ct);
    Task AddAsync(Customer customer, CancellationToken ct);
    Task UpdateAsync(Customer customer, CancellationToken ct);
    Task DeleteAsync(Customer customer, CancellationToken ct);
}