using System.Text.Json.Serialization;

namespace EcoPulseTrafficService.Models;

public class TrafficResponse
{
    [JsonPropertyName("flowSegmentData")]
    public FlowSegmentData FlowSegmentData { get; set; } = null!;
}

public class FlowSegmentData
{
    [JsonPropertyName("frc")]
    public string Frc { get; set; } = null!;

    [JsonPropertyName("currentSpeed")]
    public int CurrentSpeed { get; set; }
    
    [JsonPropertyName("freeFlowSpeed")]
    public int FreeFlowSpeed { get; set; }
    
    [JsonPropertyName("currentTravelTime")]
    public int CurrentTravelTime { get; set; }
    
    [JsonPropertyName("freeFlowTravelTime")]
    public int FreeFlowTravelTime { get; set; }
    
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
    
    [JsonPropertyName("roadClosure")]
    public bool RoadClosure { get; set; }
    
    [JsonPropertyName("coordinates")]
    public Coordinates Coordinates { get; set; } = null!;
}

public class Coordinates
{
    [JsonPropertyName("coordinate")]
    public List<Coordinate> Coordinate { get; set; } = null!;
}

public class Coordinate
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}