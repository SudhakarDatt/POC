using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Models;

public class WeatherData
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Location { get; set; } = string.Empty;

    [Required]
    public double Temperature { get; set; }

    [Required]
    public double Humidity { get; set; }

    [Required]
    public double WindSpeed { get; set; }

    [MaxLength(50)]
    public string Condition { get; set; } = string.Empty;

    [Required]
    public DateTime Timestamp { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Encrypted field for sensitive data
    public string? EncryptedData { get; set; }
}