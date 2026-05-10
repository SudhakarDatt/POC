namespace WeatherAPI.Models;

public class WeatherRequest
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
    public string Condition { get; set; } = string.Empty;
}