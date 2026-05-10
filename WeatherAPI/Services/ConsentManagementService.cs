namespace WeatherAPI.Services;

public class ConsentManagementService : IConsentManagementService
{
    private readonly ILogger<ConsentManagementService> _logger;

    public ConsentManagementService(ILogger<ConsentManagementService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> RecordConsentAsync(string userId, string consentType, bool granted)
    {
        _logger.LogInformation("Recording consent for user {UserId}", userId);
        return await Task.FromResult(true);
    }

    public async Task<bool> HasConsentAsync(string userId, string consentType)
    {
        return await Task.FromResult(true);
    }
}