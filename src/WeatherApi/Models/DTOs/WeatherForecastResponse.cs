namespace WeatherApi.Models.DTOs;

public class WeatherForecastResponse
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public List<DailyForecast> Forecast { get; set; } = new();
}

public class DailyForecast
{
    public DateTime Date { get; set; }
    public double TemperatureMax { get; set; }
    public double TemperatureMin { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
}