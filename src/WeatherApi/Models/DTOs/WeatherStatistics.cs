namespace WeatherApi.Models.DTOs;

public class WeatherStatistics
{
    public int TotalRecords { get; set; }
    public int TotalCities { get; set; }
    public double AverageTemperature { get; set; }
    public double MaxTemperature { get; set; }
    public double MinTemperature { get; set; }
    public DateTime OldestRecord { get; set; }
    public DateTime NewestRecord { get; set; }
}