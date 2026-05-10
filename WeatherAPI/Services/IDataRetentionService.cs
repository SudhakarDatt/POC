namespace WeatherAPI.Services;

public interface IDataRetentionService
{
    Task<int> ArchiveExpiredDataAsync();
    Task<int> DeleteExpiredDataAsync();
}