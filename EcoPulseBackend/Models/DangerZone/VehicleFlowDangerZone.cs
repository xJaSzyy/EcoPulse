using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Models.DangerZone;

/// <summary>
/// Зона выброса от движущегося транспорта
/// </summary>
public class VehicleFlowDangerZone
{
    /// <summary>
    /// Идентификатор источника выброса
    /// </summary>
    public int EmissionSourceId { get; set; }
    
    [Column(TypeName = "geometry(LineString, 4326)")]
    public LineString Points { get; set; } = null!;

    /// <summary>
    /// Цвет зоны выброса
    /// </summary>
    public string Color { get; set; } = null!;
    
    /// <summary>
    /// Среднее значение концентрации
    /// </summary>
    public float AverageConcentration { get; set; }
}