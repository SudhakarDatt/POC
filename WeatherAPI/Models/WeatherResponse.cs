namespace WeatherAPI.Models;

public class WeatherResponse
{
    public int Id { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
    public string Condition { get; set; } = string.Empty;
    public double Pressure { get; set; }
    public double Visibility { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime CreatedAt { get; set; }
}