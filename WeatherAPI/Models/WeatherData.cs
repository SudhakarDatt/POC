using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Models;

public class WeatherData
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    [Range(-100, 100)]
    public double Temperature { get; set; }

    [Range(0, 100)]
    public int Humidity { get; set; }

    [Range(0, 200)]
    public double WindSpeed { get; set; }

    [Required]
    [StringLength(50)]
    public string Condition { get; set; } = string.Empty;

    [Range(0, 100000)]
    public double Pressure { get; set; }

    [Range(0, 100)]
    public double Visibility { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [StringLength(100)]
    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }
}