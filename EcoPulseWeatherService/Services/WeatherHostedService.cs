using EcoPulseWeatherService.Interfaces;
using EcoPulseWeatherService.Models;
using Microsoft.Extensions.Options;

namespace EcoPulseWeatherService.Services;

public class WeatherHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval;

    public WeatherHostedService(IServiceProvider serviceProvider, 
        IOptions<WeatherServiceOptions> options)
    {
        _serviceProvider = serviceProvider;
        _interval = TimeSpan.FromMinutes(options.Value.FetchIntervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = _serviceProvider.CreateScope();
            var weatherService = scope.ServiceProvider.GetRequiredService<IWeatherService>();
            await weatherService.FetchAndSendAsync(stoppingToken);
        }
    }
}