using System.Text.Json;
using EcoPulseTrafficService.Interfaces;
using EcoPulseTrafficService.Models;

namespace EcoPulseTrafficService.Services;

public class TrafficService : ITrafficService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TrafficService> _logger;

    public TrafficService(IHttpClientFactory httpClientFactory, ILogger<TrafficService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task FetchAndSendAsync(TrafficServiceOptions options, CancellationToken ct)
    {
        var trafficClient = _httpClientFactory.CreateClient("traffic");
        var backendClient = _httpClientFactory.CreateClient("backend");

        var trafficUrl = $"https://api.tomtom.com/traffic/services/4/flowSegmentData/absolute/10/json?key={options.ApiKey}&point=55.344746,86.110833\n";

        try
        {
            var response = await trafficClient.GetAsync(trafficUrl, ct);

            var responseContent = await response.Content.ReadAsStringAsync(ct);
            var trafficResponse = JsonSerializer.Deserialize<TrafficResponse>(responseContent);

            Console.WriteLine(trafficResponse.FlowSegmentData.Frc);
            Console.WriteLine(trafficResponse.FlowSegmentData.Coordinates.Coordinate.First().Latitude);
            Console.WriteLine(trafficResponse.FlowSegmentData.Coordinates.Coordinate.First().Longitude);
            Console.WriteLine(trafficResponse.FlowSegmentData.Coordinates.Coordinate.Count);

            /*var backendResponse = await backendClient.PostAsJsonAsync("http://backend:5000/weather/save", result, ct);
            backendResponse.EnsureSuccessStatusCode();

            _logger.LogInformation($"Weather sent to backend: {currentDate}");*/
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}