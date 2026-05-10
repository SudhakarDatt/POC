namespace WeatherAPI.Models.DTOs;

public class ConsentStatus
{
    public bool DataCollectionConsent { get; set; }
    public bool DataProcessingConsent { get; set; }
    public bool MarketingConsent { get; set; }
    public DateTime ConsentDate { get; set; }
    public string ConsentVersion { get; set; } = string.Empty;
}