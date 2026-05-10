using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Models;

public class UserConsent
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public bool DataCollectionConsent { get; set; }

    [Required]
    public bool DataProcessingConsent { get; set; }

    [Required]
    public bool MarketingConsent { get; set; }

    [Required]
    public DateTime ConsentDate { get; set; }

    public DateTime? RevokedDate { get; set; }

    [MaxLength(50)]
    public string ConsentVersion { get; set; } = "1.0";

    [MaxLength(50)]
    public string? IpAddress { get; set; }
}