using EcoPulseWeatherService.Interfaces;

namespace EcoPulseWeatherService.Services;

public class WeatherHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);

    public WeatherHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
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