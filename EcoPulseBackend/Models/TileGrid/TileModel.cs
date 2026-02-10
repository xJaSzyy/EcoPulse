using System.ComponentModel.DataAnnotations.Schema;
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
}