using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Models.City;

public class CityAddModel
{
    /// <summary>
    /// Название
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Координаты
    /// </summary>
    [Column(TypeName = "geometry(Point, 4326)")]
    public Point Location { get; set; } = null!;
}