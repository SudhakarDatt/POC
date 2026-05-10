namespace WeatherApi.Models.DTOs;

public class WeatherResponse
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public double FeelsLike { get; set; }
    public int Humidity { get; set; }
    public double Pressure { get; set; }
    public string Description { get; set; } = string.Empty;
    public double WindSpeed { get; set; }
    public int? WindDirection { get; set; }
    public double? Visibility { get; set; }
    public DateTime Timestamp { get; set; }
}