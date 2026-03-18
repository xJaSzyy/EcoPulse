using System.Text;
using System.Text.Json;

namespace EcoPulseBackend.Services;

public class ConsulRegistrationService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ConsulRegistrationService> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public ConsulRegistrationService(
        IHttpClientFactory httpClientFactory,
        ILogger<ConsulRegistrationService> logger,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _environment = environment;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        
        await RegisterWithConsul(cancellationToken);
    }

    private async Task RegisterWithConsul(CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            
            var serviceAddress = _configuration.GetValue<string>("Consul:ServiceAddress") ?? "backend:5000";
            var serviceName = _configuration.GetValue<string>("Consul:ServiceName") ?? "backend";
            var healthCheckPath = _configuration.GetValue<string>("Consul:HealthCheckPath") ?? "/health";
            var serviceId = $"{serviceName}-{Guid.NewGuid():N}";
            
            var payload = new
            {
                ID = serviceId,
                Name = serviceName,
                Address = serviceAddress.Split(':')[0],
                Port = int.Parse(serviceAddress.Split(':')[1]),
                Tags = new[] { "api", _environment.EnvironmentName },
                Check = new
                {
                    HTTP = $"http://{serviceAddress}{healthCheckPath}",
                    Interval = _configuration.GetValue<string>("Consul:CheckInterval") ?? "10s",
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _logger.LogInformation("Registering service with Consul. ServiceId: {ServiceId}, Address: {Address}", 
                serviceId, serviceAddress);
            
            var response = await client.PutAsync(
                "http://consul:8500/v1/agent/service/register", 
                content, 
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully registered with Consul. ServiceId: {ServiceId}", serviceId);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to register with Consul. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering with Consul");
        }
    }
}