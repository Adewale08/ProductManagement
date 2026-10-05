using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Api.DTOs;

public class CreateCustomerDTO
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required, StringLength(100), EmailAddress]
    public string Email { get; set; } = string.Empty;

    // [Required, StringLength(15), Phone]
    public string PhoneNumber { get; set; } = string.Empty;
}