using System.Text.Json;
using EcoPulseTrafficService.Interfaces;
using EcoPulseTrafficService.Models;
using Microsoft.Extensions.Options;

namespace EcoPulseTrafficService.Services;

public class TrafficHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TrafficServiceOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public TrafficHostedService(IServiceProvider serviceProvider,
        IOptions<TrafficServiceOptions> options, IHttpClientFactory httpClientFactory)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.FetchIntervalMinutes));
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            var sourceResponse = await GetEmissionSourceResponse(stoppingToken);

            using var scope = _serviceProvider.CreateScope();
            var trafficService = scope.ServiceProvider.GetRequiredService<ITrafficService>();
            await trafficService.FetchAndSendAsync(sourceResponse, _options, stoppingToken);
        }
    }

    private async Task<List<VehicleFlowEmissionSourceResponse>?> GetEmissionSourceResponse(CancellationToken ct)
    {
        var backendClient = _httpClientFactory.CreateClient("backend");
        var response = await backendClient.GetAsync("http://backend:5000/emission-source/vehicle-flow", ct);

        var responseContent = await response.Content.ReadAsStringAsync(ct);
        var sourceResponse = JsonSerializer.Deserialize<List<VehicleFlowEmissionSourceResponse>>(responseContent);

        return sourceResponse;
    }
}