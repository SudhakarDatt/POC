using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Models;

public class AuditLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(100)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Resource { get; set; } = string.Empty;

    public string? Details { get; set; }

    [StringLength(50)]
    public string IpAddress { get; set; } = string.Empty;

    [StringLength(500)]
    public string UserAgent { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public DateTime? RetentionExpiry { get; set; }
}