using WeatherApi.Models;

namespace WeatherApi.Services;

public interface IConsentManagementService
{
    Task<UserConsent> GrantConsentAsync(string userId, string consentType, string consentText);
    Task<bool> RevokeConsentAsync(string userId, string consentType);
    Task<bool> HasConsentAsync(string userId, string consentType);
    Task<IEnumerable<UserConsent>> GetUserConsentsAsync(string userId);
}