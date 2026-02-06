using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Models.DangerZone;

public class TrafficLightQueueDangerZone
{
    /// <summary>
    /// Идентификатор источника выброса
    /// </summary>
    public int EmissionSourceId { get; set; }
    
    /// <summary>
    /// Координаты
    /// </summary>
    [Column(TypeName = "geometry(Point, 4326)")]
    public Point Location { get; set; } = null!;
    
    /// <summary>
    /// Цвет зоны выброса
    /// </summary>
    public string Color { get; set; } = null!;
    
    /// <summary>
    /// Среднее значение из n макисмальных концентраций
    /// </summary>
    public float AverageConcentration { get; set; }
}