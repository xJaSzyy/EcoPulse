using System.Text;
using System.Text.Json;
using EcoPulseBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace EcoPulseBackend.Controllers;

[ApiController]
public class DiscoveryController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DiscoveryController> _logger;

    public DiscoveryController(IHttpClientFactory httpClientFactory, ILogger<DiscoveryController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterService([FromBody] ServiceRegistrationModel model)
    {
        var client = _httpClientFactory.CreateClient();
        var payload = new
        {
            ID = $"{model.ServiceName}-{Guid.NewGuid():N}",
            Name = model.ServiceName,  
            Address = model.Address.Split(':')[0],
            Port = int.Parse(model.Address.Split(':')[1]),
            Tags = model.Tags ?? [],
            Check = new
            {
                HTTP = $"http://{model.Address}/health",
                Interval = model.CheckInterval
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PutAsync("http://consul:8500/v1/agent/service/register", content);

        _logger.LogInformation("Service {Id} registered: {Status}", payload.ID, response.StatusCode);
        return Ok();
    }

    [HttpGet("services")]
    public async Task<IActionResult> GetServices()
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"http://consul:8500/v1/catalog/services";

        var response = await client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        return Ok(JsonSerializer.Deserialize<object>(content));
    }
}