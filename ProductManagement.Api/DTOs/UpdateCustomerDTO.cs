using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Api.DTOs;

public class UpdateCustomerDTO : CreateCustomerDTO
{
    public bool IsActive { get; set; } = true;
}