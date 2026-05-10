namespace WeatherAPI.Services;

public interface IDataRetentionService
{
    Task CleanupExpiredDataAsync();
    Task<int> GetRetentionDaysAsync(string dataType);
}