using EcoPulseTrafficService.Interfaces;
using EcoPulseTrafficService.Models;
using Microsoft.Extensions.Options;

namespace EcoPulseTrafficService.Services;

public class TrafficHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TrafficServiceOptions _options;

    public TrafficHostedService(IServiceProvider serviceProvider,
        IOptions<TrafficServiceOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.FetchIntervalMinutes));
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = _serviceProvider.CreateScope();
            var trafficService = scope.ServiceProvider.GetRequiredService<ITrafficService>();
            await trafficService.FetchAndSendAsync(_options, stoppingToken);
        }
    }
}