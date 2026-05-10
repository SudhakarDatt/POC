namespace WeatherAPI.Services;

public interface IConsentManagementService
{
    Task<bool> RecordConsentAsync(string userId, string consentType, bool granted);
    Task<bool> RevokeConsentAsync(string userId, string consentType);
    Task<Dictionary<string, bool>> GetUserConsentsAsync(string userId);
    Task<bool> HasConsentAsync(string userId, string consentType);
}