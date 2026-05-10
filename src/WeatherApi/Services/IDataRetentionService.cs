namespace WeatherApi.Services;

public interface IDataRetentionService
{
    Task ArchiveExpiredDataAsync();
    Task DeleteExpiredAuditLogsAsync();
    Task<int> GetRetentionDaysAsync(string dataType);
}