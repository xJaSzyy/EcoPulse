using EcoPulseWeatherService.Interfaces;

namespace EcoPulseWeatherService.Services;

public class WeatherHostedService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<WeatherHostedService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public WeatherHostedService(IServiceProvider services, ILogger<WeatherHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = _services.CreateScope();
            var weatherService = scope.ServiceProvider.GetRequiredService<IWeatherService>();
            await weatherService.FetchAndSendAsync(stoppingToken);
        }
    }
}