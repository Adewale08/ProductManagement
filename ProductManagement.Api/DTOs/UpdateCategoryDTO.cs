using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Api.DTOs;

public class UpdateCategoryDto : CreateCategoryDto
{
    public bool IsActive { get; set; } = true;
}