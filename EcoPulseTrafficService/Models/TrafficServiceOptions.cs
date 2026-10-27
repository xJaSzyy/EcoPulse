namespace EcoPulseTrafficService.Models;

public class TrafficServiceOptions
{
    public int FetchIntervalMinutes { get; set; }
    public string ApiKey { get; set; } = null!;
}