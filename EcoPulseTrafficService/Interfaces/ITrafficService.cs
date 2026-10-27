using EcoPulseTrafficService.Models;

namespace EcoPulseTrafficService.Interfaces;

public interface ITrafficService
{
    public Task FetchAndSendAsync(List<VehicleFlowEmissionSourceResponse>? emissionSources,
        TrafficServiceOptions options,
        CancellationToken ct);
}