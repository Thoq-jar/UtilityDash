// ReSharper disable PropertyCanBeMadeInitOnly.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace Dashboard.Models;

public class WeatherData
{
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public string WeatherCondition { get; set; }
    public string IconUrl { get; set; }
    public double WindSpeed { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}