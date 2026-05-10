using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherApi.Models;

/// <summary>
/// Weather data entity with encryption support
/// </summary>
public class WeatherData
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Country { get; set; } = string.Empty;

    [Required]
    public double Temperature { get; set; }

    [Required]
    public double FeelsLike { get; set; }

    [Required]
    public int Humidity { get; set; }

    [Required]
    public double Pressure { get; set; }

    [Required]
    [MaxLength(100)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public double WindSpeed { get; set; }

    public int? WindDirection { get; set; }

    public double? Visibility { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    // Encrypted sensitive data (if needed)
    public string? EncryptedData { get; set; }

    // Data retention flag
    public bool IsArchived { get; set; } = false;

    public DateTime? ArchivedAt { get; set; }
}