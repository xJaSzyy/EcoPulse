using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Models.City;

public class CityUpdateModel
{
    /// <summary>
    /// Идентификатор
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Название
    /// </summary>
    public string? Name { get; set; } 

    /// <summary>
    /// Координаты
    /// </summary>
    [Column(TypeName = "geometry(Point, 4326)")]
    public Point? Location { get; set; } 
    
    /// <summary>
    /// Границы
    /// </summary>
    [Column(TypeName = "geometry(Polygon, 4326)")]
    public Polygon? Polygon { get; set; } 
}