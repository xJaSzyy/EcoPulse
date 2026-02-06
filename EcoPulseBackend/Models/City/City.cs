using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Models.City;

/// <summary>
/// Город
/// </summary>
public class City
{
    /// <summary>
    /// Идентификатор
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Название
    /// </summary>
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Координаты
    /// </summary>
    [Column(TypeName = "geometry(Point, 4326)")]
    public Point Location { get; set; } = null!;

    /// <summary>
    /// Границы
    /// </summary>
    [Column(TypeName = "geometry(Polygon, 4326)")]
    public Polygon Polygon { get; set; } = null!;
}