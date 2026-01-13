using System.ComponentModel.DataAnnotations;

namespace Inventory.Application.DTOs.Product;

public class CreateProductDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }  

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
    public int Status { get; set; }

    [Required]
    public int CategoryId { get; set; }

}
