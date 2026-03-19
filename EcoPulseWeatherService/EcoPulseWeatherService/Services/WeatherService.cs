using System.Text.Json;
using EcoPulseWeatherService.Interfaces;
using EcoPulseWeatherService.Models;

namespace EcoPulseWeatherService.Services;

public class WeatherService : IWeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(IHttpClientFactory httpClientFactory, ILogger<WeatherService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task FetchAndSendAsync(CancellationToken ct)
    {
        var weatherClient = _httpClientFactory.CreateClient("weather");
        var backendClient = _httpClientFactory.CreateClient("backend");

        const string weatherUrl = "https://api.open-meteo.com/v1/forecast?latitude=55.355198&longitude=86.086847&current_weather=true";

        var currentDate = DateTime.UtcNow;
        
        try
        {
            var response = await weatherClient.GetAsync(weatherUrl, ct);

            var responseContent = await response.Content.ReadAsStringAsync(ct);
            var weatherResponse = JsonSerializer.Deserialize<OpenMeteoResponse>(responseContent);

            var result = new WeatherViewModel
            {
                Date = currentDate,
                Temperature = (float)weatherResponse!.CurrentWeather.Temperature,
                WindSpeed = (float)weatherResponse.CurrentWeather.WindSpeed,
                WindDirection = (int)(weatherResponse.CurrentWeather.WindDirection + 180) % 360,
                IconClass = GetWeatherInfo(weatherResponse.CurrentWeather.WeatherCode,
                    weatherResponse.CurrentWeather.IsDay == 1).IconClass
            };
            
            var backendResponse = await backendClient.PostAsJsonAsync("/api/weather/save", result, ct);
            backendResponse.EnsureSuccessStatusCode();
        
            _logger.LogInformation($"Weather sent to backend: {currentDate}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
    
    private static (string Description, string IconClass) GetWeatherInfo(int weatherCode, bool isDay)
    {
        return weatherCode switch
        {
            0 => isDay
                ? ("Ясное небо", "wi-day-sunny")
                : ("Ясная ночь", "wi-night-clear"),

            1 => isDay
                ? ("Преимущественно ясно", "wi-day-cloudy")
                : ("Преимущественно ясно", "wi-night-alt-cloudy"),

            2 => ("Переменная облачность", "wi-cloudy"),

            3 => ("Пасмурно", "wi-cloudy"),

            45 or 48 => ("Туман", "wi-fog"),

            51 or 53 or 55 => ("Морось", "wi-sprinkle"),

            56 or 57 => ("Ледяная морось", "wi-rain-mix"),

            61 or 63 or 65 => ("Дождь", "wi-rain"),

            66 or 67 => ("Ледяной дождь", "wi-rain-mix"),

            71 or 73 or 75 => ("Снег", "wi-snow"),

            77 => ("Снежные зёрна", "wi-snow"),

            80 or 81 or 82 => ("Ливень", "wi-showers"),

            85 or 86 => ("Снегопад", "wi-snow-wind"),

            95 => ("Гроза", "wi-thunderstorm"),

            96 or 99 => ("Гроза с градом", "wi-storm-showers"),

            _ => ("Неизвестно", "wi-na")
        };
    }
}