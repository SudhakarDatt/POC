namespace WeatherAPI.Services;

public class DataRetentionService : IDataRetentionService
{
    private readonly ILogger<DataRetentionService> _logger;

    public DataRetentionService(ILogger<DataRetentionService> logger)
    {
        _logger = logger;
    }

    public async Task<int> ArchiveExpiredDataAsync()
    {
        _logger.LogInformation("Archiving expired data");
        return await Task.FromResult(0);
    }

    public async Task<int> DeleteExpiredDataAsync()
    {
        _logger.LogInformation("Deleting expired data");
        return await Task.FromResult(0);
    }
}