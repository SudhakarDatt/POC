using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Models.DTOs;

public class UpdateConsentRequest
{
    [Required]
    public bool DataCollectionConsent { get; set; }

    [Required]
    public bool DataProcessingConsent { get; set; }

    [Required]
    public bool MarketingConsent { get; set; }
}