namespace EcoPulseWeatherService.Services;

public class ConsulRegistrationService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public ConsulRegistrationService(
        IHttpClientFactory httpClientFactory,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _environment = environment;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        
        await RegisterWithConsul(cancellationToken);
    }

    private async Task RegisterWithConsul(CancellationToken ct)
    {
        var backendClient = _httpClientFactory.CreateClient("backend");
        
        var serviceAddress = _configuration.GetValue<string>("Consul:ServiceAddress") ?? "weather-service:5000";
        var serviceName = _configuration.GetValue<string>("Consul:ServiceName") ?? "weather";
        var checkInterval = _configuration.GetValue<string>("Consul:CheckInterval") ?? "10s";
            
        var payload = new
        {
            ServiceName = serviceName,
            Address = serviceAddress,
            Tags = new[] { "weather", _environment.EnvironmentName },
            CheckInterval = checkInterval
        };
        
        var backendResponse = await backendClient.PostAsJsonAsync("http://backend:5000/discovery/register", payload, ct);
        backendResponse.EnsureSuccessStatusCode();
    }
}