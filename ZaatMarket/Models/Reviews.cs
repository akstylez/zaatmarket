using System.ComponentModel.DataAnnotations;

namespace ZaatMarket.Models;

public class Review
{
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    public string UserId { get; set; } = "";

    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(600)]
    public string Comment { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}