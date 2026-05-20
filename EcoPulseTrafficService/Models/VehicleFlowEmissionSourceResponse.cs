using System.Text.Json.Serialization;
using EcoPulseBackend.Enums;

namespace EcoPulseTrafficService.Models;

public class VehicleFlowEmissionSourceResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("cityId")]
    public int CityId { get; set; }

    [JsonPropertyName("streetName")]
    public string StreetName { get; set; } = null!;

    [JsonPropertyName("points")]
    public LineStringDto Points { get; set; } = null!;

    [JsonPropertyName("vehicleType")]
    public VehicleType VehicleType { get; set; }

    [JsonPropertyName("maxTrafficIntensity")]
    public float MaxTrafficIntensity { get; set; }

    [JsonPropertyName("averageSpeed")]
    public float AverageSpeed { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }
}