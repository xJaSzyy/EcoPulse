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

        const string weatherUrl =
            "https://api.met.no/weatherapi/locationforecast/2.0/compact" +
            "?lat=55.3552&lon=86.0868";

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                weatherUrl);

            request.Headers.UserAgent.ParseAdd("EcoPulseWeatherService/1.0 (novosel.stepan@gmail.com)");

            using var response = await weatherClient.SendAsync(
                request,
                ct);

            response.EnsureSuccessStatusCode();

            await using var stream =
                await response.Content.ReadAsStreamAsync(ct);

            using var document = await JsonDocument.ParseAsync(
                stream,
                cancellationToken: ct);

            var root = document.RootElement;

            var timeseries = root
                .GetProperty("properties")
                .GetProperty("timeseries");

            var forecast = timeseries[0];

            var forecastTime = forecast
                .GetProperty("time")
                .GetDateTime();

            var instant = forecast
                .GetProperty("data")
                .GetProperty("instant")
                .GetProperty("details");

            var temperature = instant
                .GetProperty("air_temperature")
                .GetSingle();

            var windSpeed = instant
                .GetProperty("wind_speed")
                .GetSingle();

            var windDirection = (int)Math.Round(
                instant.GetProperty("wind_from_direction").GetDouble());

            windDirection = ((windDirection + 180) % 360 + 360) % 360;

            var data = forecast.GetProperty("data");
            var symbolCode = GetSymbolCode(data);

            var result = new WeatherViewModel
            {
                Date = forecastTime,
                Temperature = temperature,
                WindSpeed = windSpeed,
                WindDirection = windDirection,
                IconClass = GetIconClass(symbolCode)
            };

            using var backendResponse =
                await backendClient.PostAsJsonAsync(
                    "http://backend:5000/weather/save",
                    result,
                    ct);

            backendResponse.EnsureSuccessStatusCode();

            _logger.LogInformation(
                "Weather sent to backend: {Date}",
                forecastTime);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch or send weather data.");
        }
    }

    private static string GetSymbolCode(JsonElement data)
    {
        foreach (var period in new[]
                 {
                     "next_1_hours",
                     "next_6_hours",
                     "next_12_hours"
                 })
        {
            if (data.TryGetProperty(period, out var forecastPeriod) &&
                forecastPeriod.TryGetProperty("summary", out var summary) &&
                summary.TryGetProperty("symbol_code", out var symbol))
            {
                return symbol.GetString() ?? "unknown";
            }
        }

        return "unknown";
    }

    private static string GetIconClass(string symbolCode)
    {
        if (symbolCode.StartsWith("clearsky_day"))
            return "wi-day-sunny";

        if (symbolCode.StartsWith("clearsky_night"))
            return "wi-night-clear";

        if (symbolCode.StartsWith("fair_day"))
            return "wi-day-sunny";

        if (symbolCode.StartsWith("fair_night"))
            return "wi-night-clear";

        if (symbolCode.Contains("thunder"))
            return "wi-thunderstorm";

        if (symbolCode.Contains("snow"))
            return "wi-snow";

        if (symbolCode.Contains("sleet"))
            return "wi-rain-mix";

        if (symbolCode.Contains("rain"))
            return "wi-rain";

        if (symbolCode.Contains("fog"))
            return "wi-fog";

        if (symbolCode.Contains("cloud"))
            return "wi-cloudy";

        return "wi-na";
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