using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Api.DTOs;

public record CustomerResponseDto(
    int Id,
    string Name,
    string Email,
    string PhoneNumber,
    DateTime CreatedAtUtc);