using EcoPulseTrafficService.Models;

namespace EcoPulseTrafficService.Interfaces;

public interface ITrafficService
{
    public Task FetchAndSendAsync(TrafficServiceOptions options, CancellationToken ct);
}