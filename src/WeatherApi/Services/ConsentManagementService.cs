using Microsoft.EntityFrameworkCore;
using WeatherApi.Data;
using WeatherApi.Models;

namespace WeatherApi.Services;

/// <summary>
/// Consent management service for GDPR/CCPA compliance
/// </summary>
public class ConsentManagementService : IConsentManagementService
{
    private readonly WeatherDbContext _context;
    private readonly ILogger<ConsentManagementService> _logger;

    public ConsentManagementService(
        WeatherDbContext context,
        ILogger<ConsentManagementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserConsent> GrantConsentAsync(string userId, string consentType, string consentText)
    {
        try
        {
            var existingConsent = await _context.UserConsents
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ConsentType == consentType && c.IsGranted);

            if (existingConsent != null)
            {
                _logger.LogInformation("Consent {ConsentType} already granted for user {UserId}", consentType, userId);
                return existingConsent;
            }

            var consent = new UserConsent
            {
                UserId = userId,
                ConsentType = consentType,
                IsGranted = true,
                GrantedAt = DateTime.UtcNow,
                ConsentText = consentText
            };

            _context.UserConsents.Add(consent);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Consent {ConsentType} granted for user {UserId}", consentType, userId);

            return consent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error granting consent for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> RevokeConsentAsync(string userId, string consentType)
    {
        try
        {
            var consent = await _context.UserConsents
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ConsentType == consentType && c.IsGranted);

            if (consent == null)
            {
                _logger.LogWarning("No active consent {ConsentType} found for user {UserId}", consentType, userId);
                return false;
            }

            consent.IsGranted = false;
            consent.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Consent {ConsentType} revoked for user {UserId}", consentType, userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking consent for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> HasConsentAsync(string userId, string consentType)
    {
        try
        {
            return await _context.UserConsents
                .AnyAsync(c => c.UserId == userId && c.ConsentType == consentType && c.IsGranted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking consent for user {UserId}", userId);
            return false;
        }
    }

    public async Task<IEnumerable<UserConsent>> GetUserConsentsAsync(string userId)
    {
        try
        {
            return await _context.UserConsents
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.GrantedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving consents for user {UserId}", userId);
            throw;
        }
    }
}