using EcoPulseWeatherService.Interfaces;
using EcoPulseWeatherService.Services;

namespace EcoPulseWeatherService;

public static class DependencyInjection
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IWeatherService, WeatherService>();
        services.AddHostedService<WeatherHostedService>();
        services.AddHostedService<ConsulRegistrationService>();
    }
}