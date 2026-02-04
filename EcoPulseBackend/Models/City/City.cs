using System.ComponentModel.DataAnnotations;
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
    public Coordinates Location { get; set; } = null!;
}