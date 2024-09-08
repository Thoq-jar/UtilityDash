using Dashboard.Models;
using Newtonsoft.Json.Linq;

namespace Dashboard.Services;

public class WeatherService(HttpClient httpClient)
{
    private const string IpApiUrl = "http://ip-api.com/json";
    private const string OpenMeteoUrl = "https://api.open-meteo.com/v1/forecast";

public async Task<WeatherData> GetWeatherAsync()
{
    try
    {
        var ipResponse = await httpClient.GetStringAsync(IpApiUrl);
        var locationData = JObject.Parse(ipResponse);

        var latitude = locationData["lat"]?.ToString();
        var longitude = locationData["lon"]?.ToString();

        if (string.IsNullOrWhiteSpace(latitude) || string.IsNullOrWhiteSpace(longitude))
        {
            throw new Exception("Unable to retrieve location data.");
        }

        var weatherUrl = $"{OpenMeteoUrl}?latitude={latitude}&longitude={longitude}&hourly=temperature_2m,relative_humidity_2m,weathercode,windspeed_10m";
        var weatherResponse = await httpClient.GetStringAsync(weatherUrl);
        var weatherData = JObject.Parse(weatherResponse);
        
        if (weatherData["hourly"]?["temperature_2m"] is not JArray temperatureArray || 
            weatherData["hourly"]?["relative_humidity_2m"] is not JArray humidityArray || 
            weatherData["hourly"]?["weathercode"] is not JArray weatherCodeArray ||
            weatherData["hourly"]?["windspeed_10m"] is not JArray windSpeedArray ||
            temperatureArray.Count == 0 || humidityArray.Count == 0 || weatherCodeArray.Count == 0 || windSpeedArray.Count == 0)
        {
            throw new Exception("Weather data is incomplete.");
        }

        var temperatureC = temperatureArray[0].Value<double>();
        var temperature = temperatureC * 9 / 5 + 32;
        var humidity = humidityArray[0].Value<double>();
        var weatherConditionCode = weatherCodeArray[0].ToString();
        var windSpeed = windSpeedArray[0].Value<double>();

        var (weatherCondition, iconUrl) = GetWeatherConditionDescription(weatherConditionCode);

        return new WeatherData
        {
            Temperature = temperature,
            Humidity = humidity,
            WeatherCondition = weatherCondition,
            IconUrl = iconUrl,
            WindSpeed = windSpeed,
            Latitude = double.Parse(latitude),
            Longitude = double.Parse(longitude)
        };
    }
    catch (HttpRequestException httpEx)
    {
        Console.WriteLine($"HTTP request error: {httpEx.Message}");
        return new WeatherData
        {
            Temperature = double.NaN,
            Humidity = double.NaN,
            WeatherCondition = "Unknown",
            WindSpeed = double.NaN,
            Latitude = double.NaN,
            Longitude = double.NaN
        };
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error retrieving weather data: {ex.Message}");
        return new WeatherData
        {
            Temperature = double.NaN,
            Humidity = double.NaN,
            WeatherCondition = "Unknown",
            WindSpeed = double.NaN,
            Latitude = double.NaN,
            Longitude = double.NaN
        };
    }
}
    private static (string description, string iconUrl) GetWeatherConditionDescription(string weatherCode)
    {
        return weatherCode switch
        {
            "0" => ("Clear sky", "http://openweathermap.org/img/wn/01d@2x.png"),
            "1" => ("Mainly clear", "http://openweathermap.org/img/wn/02d@2x.png"),
            "2" => ("Partly cloudy", "http://openweathermap.org/img/wn/03d@2x.png"),
            "3" => ("Overcast", "http://openweathermap.org/img/wn/04d@2x.png"),
            "45" => ("Fog", "http://openweathermap.org/img/wn/50d@2x.png"),
            "48" => ("Depositing rime fog", "http://openweathermap.org/img/wn/50d@2x.png"),
            "51" => ("Light drizzle", "http://openweathermap.org/img/wn/09d@2x.png"),
            "53" => ("Moderate drizzle", "http://openweathermap.org/img/wn/09d@2x.png"),
            "55" => ("Dense drizzle", "http://openweathermap.org/img/wn/09d@2x.png"),
            "61" => ("Light rain", "http://openweathermap.org/img/wn/10d@2x.png"),
            "63" => ("Moderate rain", "http://openweathermap.org/img/wn/10d@2x.png"),
            "65" => ("Heavy rain", "http://openweathermap.org/img/wn/10d@2x.png"),
            "71" => ("Light snow", "http://openweathermap.org/img/wn/13d@2x.png"),
            "73" => ("Moderate snow", "http://openweathermap.org/img/wn/13d@2x.png"),
            "75" => ("Heavy snow", "http://openweathermap.org/img/wn/13d@2x.png"),
            "95" => ("Thunderstorms", "http://openweathermap.org/img/wn/11d@2x.png"),
            "96" => ("Thunderstorms with hail", "http://openweathermap.org/img/wn/11d@2x.png"),
            "99" => ("Thunderstorms with heavy hail", "http://openweathermap.org/img/wn/11d@2x.png"),
            _ => ("Unknown", "")
        };
    }
}