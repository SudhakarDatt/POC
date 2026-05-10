namespace WeatherAPI.Services;

public class DataRetentionService : IDataRetentionService
{
    private readonly ILogger<DataRetentionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, int> _retentionPolicies = new();

    public DataRetentionService(
        ILogger<DataRetentionService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        InitializeDefaultPolicies();
    }

    private void InitializeDefaultPolicies()
    {
        var retentionDays = _configuration.GetValue<int>("DataRetention:RetentionDays", 90);
        var archiveDays = _configuration.GetValue<int>("DataRetention:ArchiveAfterDays", 365);

        _retentionPolicies["WeatherData"] = retentionDays;
        _retentionPolicies["AuditLogs"] = archiveDays;
        _retentionPolicies["UserConsent"] = 730;
    }

    public async Task<int> ArchiveExpiredDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting data archival process");
            
            var archivedCount = 0;
            var archiveDays = _configuration.GetValue<int>("DataRetention:ArchiveAfterDays", 365);
            var archiveDate = DateTime.UtcNow.AddDays(-archiveDays);

            _logger.LogInformation(
                "Archiving data older than {ArchiveDate}. Total records to archive: {Count}",
                archiveDate, archivedCount);

            await Task.CompletedTask;
            return archivedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data archival");
            throw;
        }
    }

    public async Task<int> DeleteExpiredDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting expired data deletion process");
            
            var deletedCount = 0;
            var retentionDays = _configuration.GetValue<int>("DataRetention:RetentionDays", 90);
            var expiryDate = DateTime.UtcNow.AddDays(-retentionDays);

            _logger.LogInformation(
                "Deleting data older than {ExpiryDate}. Total records deleted: {Count}",
                expiryDate, deletedCount);

            await Task.CompletedTask;
            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during expired data deletion");
            throw;
        }
    }

    public async Task<bool> SetRetentionPolicyAsync(string dataType, int retentionDays)
    {
        try
        {
            if (retentionDays < 1)
            {
                _logger.LogWarning("Invalid retention days: {Days}", retentionDays);
                return false;
            }

            _retentionPolicies[dataType] = retentionDays;
            
            _logger.LogInformation(
                "Updated retention policy for {DataType}: {Days} days",
                dataType, retentionDays);

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting retention policy for {DataType}", dataType);
            throw;
        }
    }
}