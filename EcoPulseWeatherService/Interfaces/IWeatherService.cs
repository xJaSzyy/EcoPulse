namespace EcoPulseWeatherService.Interfaces;

public interface IWeatherService
{
    public Task FetchAndSendAsync(CancellationToken ct);
}