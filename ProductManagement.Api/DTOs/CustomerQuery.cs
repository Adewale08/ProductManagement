using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Api.DTOs;

public class CustomerQuery
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? CreatedAtUtc { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}