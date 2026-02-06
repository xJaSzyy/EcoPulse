using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Models.DangerZone;

/// <summary>
/// Зона выброса от одиночного точечного источника
/// </summary>
public class SingleDangerZone
{
    /// <summary>
    /// Идентификатор источника выброса
    /// </summary>
    public int EmissionSourceId { get; set; }
    
    [Column(TypeName = "geometry(Point, 4326)")]
    public Point Location { get; set; } = null!;
    
    /// <summary>
    /// Длина зоны выброса
    /// </summary>
    public double Length { get; set; }
    
    /// <summary>
    /// Ширина зоны выброса
    /// </summary>
    public double Width { get; set; }
    
    /// <summary>
    /// Цвет зоны выброса
    /// </summary>
    public string Color { get; set; } = null!;
    
    /// <summary>
    /// Среднее значение из n макисмальных концентраций
    /// </summary>
    public float AverageConcentration { get; set; }
    
    /// <summary>
    /// Угол направления
    /// </summary>
    public double Angle { get; set; }

    /// <summary>
    /// Уровень загрязнения
    /// </summary>
    public string PollutionLevel { get; set; } = null!;
}