using System.ComponentModel.DataAnnotations;

namespace WeatherApi.Models;

/// <summary>
/// Audit log entity for compliance and security tracking
/// </summary>
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
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    public string? Details { get; set; }

    public bool IsSuccessful { get; set; } = true;

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    // Compliance fields
    public bool IsRetained { get; set; } = true;

    public DateTime? RetentionExpiryDate { get; set; }
}