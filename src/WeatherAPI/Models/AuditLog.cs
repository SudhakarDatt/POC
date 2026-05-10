using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Models;

public class AuditLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Resource { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Result { get; set; } = string.Empty;

    [Required]
    public DateTime Timestamp { get; set; }

    [MaxLength(500)]
    public string? Details { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(200)]
    public string? UserAgent { get; set; }
}