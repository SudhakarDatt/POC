namespace WeatherAPI.Services;

public class ConsentManagementService : IConsentManagementService
{
    private readonly ILogger<ConsentManagementService> _logger;
    private readonly Dictionary<string, Dictionary<string, ConsentRecord>> _consents = new();

    public ConsentManagementService(ILogger<ConsentManagementService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> RecordConsentAsync(string userId, string consentType, bool granted)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(consentType))
            {
                _logger.LogWarning("Invalid consent parameters");
                return false;
            }

            if (!_consents.ContainsKey(userId))
            {
                _consents[userId] = new Dictionary<string, ConsentRecord>();
            }

            _consents[userId][consentType] = new ConsentRecord
            {
                ConsentType = consentType,
                Granted = granted,
                Timestamp = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddYears(2)
            };

            _logger.LogInformation(
                "Recorded consent for user {UserId}: {ConsentType} = {Granted}",
                userId, consentType, granted);

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording consent for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> RevokeConsentAsync(string userId, string consentType)
    {
        try
        {
            if (!_consents.ContainsKey(userId) || !_consents[userId].ContainsKey(consentType))
            {
                _logger.LogWarning("Consent not found for user {UserId}, type {ConsentType}", userId, consentType);
                return false;
            }

            _consents[userId][consentType].Granted = false;
            _consents[userId][consentType].RevokedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Revoked consent for user {UserId}: {ConsentType}",
                userId, consentType);

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking consent for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Dictionary<string, bool>> GetUserConsentsAsync(string userId)
    {
        try
        {
            if (!_consents.ContainsKey(userId))
            {
                return new Dictionary<string, bool>();
            }

            var result = _consents[userId]
                .Where(c => c.Value.ExpiryDate > DateTime.UtcNow)
                .ToDictionary(c => c.Key, c => c.Value.Granted);

            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving consents for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> HasConsentAsync(string userId, string consentType)
    {
        try
        {
            if (!_consents.ContainsKey(userId) || !_consents[userId].ContainsKey(consentType))
            {
                return false;
            }

            var consent = _consents[userId][consentType];
            var isValid = consent.Granted && 
                         consent.ExpiryDate > DateTime.UtcNow && 
                         consent.RevokedAt == null;

            return await Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking consent for user {UserId}", userId);
            throw;
        }
    }

    private class ConsentRecord
    {
        public string ConsentType { get; set; } = string.Empty;
        public bool Granted { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime? RevokedAt { get; set; }
    }
}