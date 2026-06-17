using NBomber.CSharp;
using NBomber.Http.CSharp;
using System.Text.Json;
using EcoPulseBackend.Enums;
using System.Text;
using System.Text.Json.Serialization;
using EcoPulseBackend.Models.DangerZone;

namespace EcoPulseBackendTests;

[Ignore("LoadTests")]
public class LoadTests
{
    private const string BaseUrl = "http://localhost:3000/api";

    [Test]
    public void CalculateSingleDangerZones_LoadTest()
    {
        var client = new HttpClient();

        var scenario = Scenario.Create("CalculateSingleDangerZones", async _ =>
            {
                var model = new SingleDangerZoneCalculateModel
                {
                    Pollutant = Pollutant.SP,
                    AirTemp = 13.6f,
                    WindDirection = 107,
                    WindSpeed = 14.4f,
                    CityIds = [1]
                };

                var options = new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
                };

                var json = JsonSerializer.Serialize(model, options);

                var request = Http.CreateRequest("POST", BaseUrl + "/danger-zones/single")
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(new StringContent(json, Encoding.UTF8, "application/json"));

                var response = await Http.Send(client, request);

                return response.IsError
                    ? Response.Fail()
                    : Response.Ok();
            })
            .WithLoadSimulations(
                Simulation.RampingConstant(copies: 50, during: TimeSpan.FromSeconds(30))
            );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }

    [Test]
    public void CalculateVehicleFlowDangerZones_LoadTest()
    {
        var client = new HttpClient();

        var scenario = Scenario.Create("CalculateVehicleFlowDangerZones", async _ =>
            {
                var model = new VehicleFlowDangerZoneCalculateModel
                {
                    CityIds = [1]
                };

                var options = new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
                };

                var json = JsonSerializer.Serialize(model, options);

                var request = Http.CreateRequest("POST", BaseUrl + "/danger-zones/vehicle-flow")
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(new StringContent(json, Encoding.UTF8, "application/json"));

                var response = await Http.Send(client, request);

                return response.IsError
                    ? Response.Fail()
                    : Response.Ok();
            })
            .WithLoadSimulations(
                Simulation.RampingConstant(copies: 50, during: TimeSpan.FromSeconds(30))
            );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }

    [Test]
    public void CalculateTrafficLightQueueDangerZones_LoadTest()
    {
        var client = new HttpClient();

        var scenario = Scenario.Create("CalculateTrafficLightQueueDangerZones", async _ =>
            {
                var model = new TrafficLightQueueDangerZoneCalculateModel
                {
                    CityIds = [1]
                };

                var options = new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
                };

                var json = JsonSerializer.Serialize(model, options);

                var request = Http.CreateRequest("POST", BaseUrl + "/danger-zones/traffic-light-queue")
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(new StringContent(json, Encoding.UTF8, "application/json"));

                var response = await Http.Send(client, request);

                return response.IsError
                    ? Response.Fail()
                    : Response.Ok();
            })
            .WithLoadSimulations(
                Simulation.RampingConstant(copies: 50, during: TimeSpan.FromSeconds(30))
            );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }
}