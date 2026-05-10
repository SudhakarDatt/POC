namespace WeatherAPI.Services;

public interface IConsentManagementService
{
    Task<bool> RecordConsentAsync(string userId, string consentType, bool granted);
    Task<bool> HasConsentAsync(string userId, string consentType);
}