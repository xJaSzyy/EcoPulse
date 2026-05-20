using System.Text.Json;
using EcoPulseBackend.Models.VehicleFlowEmissionSource;
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

    public async Task FetchAndSendAsync(List<VehicleFlowEmissionSourceResponse>? emissionSources,
        TrafficServiceOptions options, CancellationToken ct)
    {
        if (emissionSources is null)
        {
            return;
        }
        
        var trafficClient = _httpClientFactory.CreateClient("traffic");
        var backendClient = _httpClientFactory.CreateClient("backend");
        
        var trafficBaseUrl = $"https://api.tomtom.com/traffic/services/4/flowSegmentData/absolute/10/json?key={options.ApiKey}";
        
        foreach (var source in emissionSources)
        {
            try
            {
                var response = await trafficClient.GetAsync(trafficBaseUrl + $"&point={source.Points.Coordinates[0][1]},{source.Points.Coordinates[0][0]}", ct);

                var responseContent = await response.Content.ReadAsStringAsync(ct);
                var trafficResponse = JsonSerializer.Deserialize<TrafficResponse>(responseContent);

                if (trafficResponse is null)
                {
                    _logger.LogError($"Failed to fetch traffic response: {responseContent}");
                    continue;
                }
                
                var updateModel = new VehicleFlowEmissionSourceUpdateModel
                {
                    Id = source.Id,
                    AverageSpeed = trafficResponse.FlowSegmentData.CurrentSpeed,
                    MaxTrafficIntensity = trafficResponse.FlowSegmentData.CurrentTravelTime / 13f
                };
                
                var backendResponse = await backendClient.PutAsJsonAsync("http://backend:5000/emission-source/vehicle-flow", updateModel, ct);
                backendResponse.EnsureSuccessStatusCode();

                _logger.LogInformation($"Emission source with Id: {updateModel.Id} updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}