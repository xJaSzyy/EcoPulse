using System.Text.Json.Serialization;

namespace EcoPulseTrafficService.Models;

public class LineStringDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;

    [JsonPropertyName("coordinates")]
    public List<List<double>> Coordinates { get; set; } = null!;
}