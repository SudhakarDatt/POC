using System.ComponentModel.DataAnnotations;

namespace WeatherApi.Models;

/// <summary>
/// User consent management for GDPR/CCPA compliance
/// </summary>
public class UserConsent
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ConsentType { get; set; } = string.Empty;

    [Required]
    public bool IsGranted { get; set; }

    [Required]
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAt { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    public string? ConsentText { get; set; }

    [MaxLength(50)]
    public string Version { get; set; } = "1.0";
}