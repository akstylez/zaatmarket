using System.ComponentModel.DataAnnotations;

namespace ZaatMarket.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Title { get; set; } = "";

    [Required, StringLength(500)]
    public string Description { get; set; } = "";

    public string Location { get; set; } = string.Empty;

    public string? ImagePath { get; set; }

    [Range(0.01, 1_000_000)]
    public decimal Price { get; set; }

    [StringLength(60)]
    public string Category { get; set; } = "";

    public string OwnerUserId { get; set; } = "";

    [StringLength(200)]
    public string? ImageUrl { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Range(0, 100000, ErrorMessage = "Stock must be between 0 and 100,000")]
    public int StockQuantity { get; set; } = 1;
}