using EcoPulseBackend.Models.DangerZone;

namespace EcoPulseBackend.Models.TileGrid;

public class TileGridCalculateModel
{
    public int CityId { get; set; }
    
    public double TileSize { get; set; }
    
    public List<VehicleFlowDangerZone> VehicleFlowDangerZones { get; set; } = null!;

    public List<SingleDangerZone> SingleDangerZones { get; set; } = null!;
    
    public List<TrafficLightQueueDangerZone> TrafficLightQueueDangerZones { get; set; } = null!;
}