using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

public class WeatherTools
{
    private readonly ILogger<WeatherTools> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public WeatherTools(ILogger<WeatherTools> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        _apiKey = Environment.GetEnvironmentVariable("OPENWEATHERMAP_API_KEY")
                  ?? throw new InvalidOperationException("OPENWEATHERMAP_API_KEY is not set.");
    }

    [McpServerTool]
    [Description("Gets current weather conditions for the specified city.")]
    public async Task<string> GetCurrentWeather(
        [Description("The city name to get weather for")] string city,
        [Description("Optional: Country code (e.g., 'US', 'UK')")] string? countryCode = null)
    {
        try
        {
            var location = string.IsNullOrWhiteSpace(countryCode) ? city : $"{city},{countryCode}";
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(location)}&appid={_apiKey}&units=metric";

            var response = await _httpClient.GetFromJsonAsync<CurrentWeatherResponse>(url);
            if (response == null || response.Main == null)
                return $"No weather data found for {location}.";

            return $"Current weather in {response.Name}: {response.Main.Temp}°C, {response.Weather[0].Description}, Humidity: {response.Main.Humidity}%.";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error retrieving current weather.");
            return $"Error retrieving weather: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving current weather.");
            return $"Unexpected error: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Gets a 3-day weather forecast for the specified city.")]
    public async Task<string> GetWeatherForecast(
        [Description("The city name to get forecast for")] string city,
        [Description("Optional: Country code (e.g., 'US', 'UK')")] string? countryCode = null)
    {
        try
        {
            var location = string.IsNullOrWhiteSpace(countryCode) ? city : $"{city},{countryCode}";
            var url = $"https://api.openweathermap.org/data/2.5/forecast?q={Uri.EscapeDataString(location)}&appid={_apiKey}&units=metric";

            var response = await _httpClient.GetFromJsonAsync<ForecastResponse>(url);
            if (response == null || response.List == null || response.List.Count == 0)
                return $"No forecast data found for {location}.";

            var forecast = new List<string>();
            var currentDate = DateTime.MinValue;
            var dailyForecasts = new List<ForecastItem>();

            // Group forecasts by day (API returns 3-hour intervals)
            foreach (var item in response.List.Take(24)) // Take first 24 entries (3 days worth)
            {
                var forecastDate = UnixTimeToDateTime(item.Dt).Date;
                
                if (forecastDate != currentDate)
                {
                    // Process previous day if we have data
                    if (dailyForecasts.Any())
                    {
                        ProcessDailyForecast(forecast, currentDate, dailyForecasts);
                        dailyForecasts.Clear();
                    }
                    currentDate = forecastDate;
                }
                
                dailyForecasts.Add(item);
            }

            // Process the last day
            if (dailyForecasts.Any())
            {
                ProcessDailyForecast(forecast, currentDate, dailyForecasts);
            }

            return $"3-day weather forecast for {response.City.Name}:\n\n{string.Join("\n\n", forecast)}";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error retrieving weather forecast.");
            return $"Error retrieving forecast: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving weather forecast.");
            return $"Unexpected error: {ex.Message}";
        }
    }

    private static void ProcessDailyForecast(List<string> forecast, DateTime date, List<ForecastItem> dailyForecasts)
    {
        var minTemp = dailyForecasts.Min(f => f.Main.TempMin);
        var maxTemp = dailyForecasts.Max(f => f.Main.TempMax);
        var avgHumidity = dailyForecasts.Average(f => f.Main.Humidity);
        
        // Get the most common weather description for the day
        var weatherDescription = dailyForecasts
            .GroupBy(f => f.Weather[0].Description)
            .OrderByDescending(g => g.Count())
            .First().Key;

        forecast.Add($"{date:dddd, MMMM dd}: {minTemp:F1}°C - {maxTemp:F1}°C, {weatherDescription}, Avg Humidity: {avgHumidity:F0}%");
    }

    [McpServerTool]
    [Description("Gets current weather alerts/warnings for the specified coordinates.")]
    public async Task<string> GetWeatherAlerts(
        [Description("Latitude of the location")] double latitude,
        [Description("Longitude of the location")] double longitude)
    {
        try
        {
            var url = $"https://api.openweathermap.org/data/3.0/onecall?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric";

            var response = await _httpClient.GetFromJsonAsync<OneCallResponse>(url);
            if (response == null || response.Alerts == null || response.Alerts.Count == 0)
                return $"No weather alerts for coordinates: {latitude}, {longitude}.";

            var alertsText = string.Join("\n\n", response.Alerts.Select(a =>
            {
                var description = string.IsNullOrWhiteSpace(a.Description) ? "No description provided" : a.Description;
                var tags = a.Tags != null && a.Tags.Count > 0 ? $" [{string.Join(", ", a.Tags)}]" : "";
                return $"{a.Event}{tags} (from {a.SenderName})\nFrom: {UnixTimeToDateTime(a.Start):g} To: {UnixTimeToDateTime(a.End):g}\n{description}";
            }));

            return $"Weather alerts for {latitude}, {longitude}:\n\n{alertsText}";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error retrieving alerts.");
            return $"Error retrieving alerts: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving alerts.");
            return $"Unexpected error: {ex.Message}";
        }
    }

    private static DateTime UnixTimeToDateTime(long unixTime)
    {
        return DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
    }
}

#region DTOs
public class CurrentWeatherResponse
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("weather")] public List<WeatherDescription> Weather { get; set; } = new();
    [JsonPropertyName("main")] public MainWeatherData Main { get; set; } = new();
}

public class ForecastResponse
{
    [JsonPropertyName("list")] public List<ForecastItem> List { get; set; } = new();
    [JsonPropertyName("city")] public ForecastCity City { get; set; } = new();
}

public class ForecastCity
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}

public class ForecastItem
{
    [JsonPropertyName("dt")] public long Dt { get; set; }
    [JsonPropertyName("main")] public MainWeatherData Main { get; set; } = new();
    [JsonPropertyName("weather")] public List<WeatherDescription> Weather { get; set; } = new();
    [JsonPropertyName("dt_txt")] public string DtTxt { get; set; } = "";
}

public class WeatherDescription
{
    [JsonPropertyName("description")] public string Description { get; set; } = "";
}

public class MainWeatherData
{
    [JsonPropertyName("temp")] public double Temp { get; set; }
    [JsonPropertyName("temp_min")] public double TempMin { get; set; }
    [JsonPropertyName("temp_max")] public double TempMax { get; set; }
    [JsonPropertyName("humidity")] public int Humidity { get; set; }
}

public class OneCallResponse
{
    [JsonPropertyName("alerts")] public List<WeatherAlert> Alerts { get; set; } = new();
}

public class WeatherAlert
{
    [JsonPropertyName("sender_name")] public string SenderName { get; set; } = "";
    [JsonPropertyName("event")] public string Event { get; set; } = "";
    [JsonPropertyName("start")] public long Start { get; set; }
    [JsonPropertyName("end")] public long End { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("tags")] public List<string> Tags { get; set; } = new();
}
#endregion