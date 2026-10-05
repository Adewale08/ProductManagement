using Microsoft.EntityFrameworkCore;
using ProductManagement.Api.Data;   
using ProductManagement.Api.DTOs;
using ProductManagement.Api.Models;
using ProductManagement.Api.Repositories;
using ProductManagement.Api.Exceptions;
using System.Text.RegularExpressions;

namespace ProductManagement.Api.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    private static CustomerResponseDto MapToResponse(Customer customer)
        => new(
            customer.Id,
            customer.Name,
            customer.Email,
            customer.PhoneNumber,
            customer.CreatedAtUtc);

    public async Task<PagedResult<CustomerResponseDto>> GetAllAsync(CustomerQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await _customerRepository.GetAllAsync(request, ct);
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        var responseItems = items.Select(c => new CustomerResponseDto(
            c.Id,
            c.Name,
            c.Email,
            c.PhoneNumber,
            c.CreatedAtUtc
        )).ToList();

        return new PagedResult<CustomerResponseDto>(responseItems, request.Page, request.PageSize, totalCount, totalPages);
    }

    public async Task<CustomerResponseDto?> GetByIdAsync(int Id, CancellationToken ct)
    {
        var customer = await _customerRepository.GetByIdAsync(Id, ct);
        if (customer == null)
        {
            return null;
        }

        return new CustomerResponseDto(
            customer.Id,
            customer.Name,
            customer.Email,
            customer.PhoneNumber,
            customer.CreatedAtUtc
        );
    }

    public async Task<CustomerResponseDto> CreateAsync(CreateCustomerDTO request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ValidationException("Email is required.");
        }

        if (!Regex.IsMatch(
                request.Email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            throw new ValidationException(
                "Please provide a valid email address.");
        }

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            throw new ValidationException(
                "Phone number is required.");
        }

        if (!Regex.IsMatch(
                request.PhoneNumber,
                @"^(\+234|0)[789][01]\d{8}$"))
        {
            throw new ValidationException(
                "Please provide a valid Nigerian phone number.");
        }

        var customer = new Customer
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        await _customerRepository.AddAsync(customer, ct);

        return MapToResponse(customer);
    }

    public async Task<bool> UpdateAsync(int Id, UpdateCustomerDTO request, CancellationToken ct)
    {
        var customer = await _customerRepository.GetByIdAsync(Id, ct);
        if (customer == null)
        {
            return false;
        }

        customer.Name = request.Name;
        customer.Email = request.Email;
        customer.PhoneNumber = request.PhoneNumber;

        await _customerRepository.UpdateAsync(customer, ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int Id, CancellationToken ct)
    {
        var customer = await _customerRepository.GetByIdAsync(Id, ct);
        if (customer == null)
        {
            return false;
        }

        await _customerRepository.DeleteAsync(customer, ct);
        return true;
    }

}