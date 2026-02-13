using System.ComponentModel.DataAnnotations.Schema;
using EcoPulseBackend.Models.DangerZone;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Models.TileGrid;

/// <summary>
/// Модель тайла
/// </summary>
public class TileModel
{
    /// <summary>
    /// Полигон
    /// </summary>
    [Column(TypeName = "geometry(Polygon, 4326)")]
    public Polygon Tile { get; set; } = null!;

    /// <summary>
    /// Цвет
    /// </summary>
    public string Color { get; set; } = null!;
    
    /// <summary>
    /// Среднее значение концентрации
    /// </summary>
    public float AverageConcentration { get; set; }
    
    public List<SingleDangerZone> SingleDangerZones { get; set; } = new();
    
    public List<VehicleFlowDangerZone> VehicleFlowDangerZones { get; set; } = new();
    
    public List<TrafficLightQueueDangerZone> VehicleQueueDangerZones { get; set; } = new();
}