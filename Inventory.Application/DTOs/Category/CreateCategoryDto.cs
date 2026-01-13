using System.ComponentModel.DataAnnotations;

namespace Inventory.Application.DTOs.Category;

public class CreateCategoryDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
}
