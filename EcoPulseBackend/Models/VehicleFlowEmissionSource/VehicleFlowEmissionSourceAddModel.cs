using System.ComponentModel.DataAnnotations.Schema;
using EcoPulseBackend.Enums;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Models.VehicleFlowEmissionSource;

public class VehicleFlowEmissionSourceAddModel
{
    /// <summary>
    /// Идентификатор города
    /// </summary>
    public int CityId { get; set; }
    
    /// <summary>
    /// Список координат
    /// </summary>
    [Column(TypeName = "geometry(LineString, 4326)")]
    public LineString Points { get; set; } = null!;
    
    /// <summary>
    /// Тип транспортного средства
    /// </summary>
    public VehicleType VehicleType { get; set; }
        
    /// <summary>
    /// Фактическая наибольшая интенсивность движения
    /// </summary>
    public float MaxTrafficIntensity { get; set; }
        
    /// <summary>
    /// Средняя скорость движения транспортного потока
    /// </summary>
    public float AverageSpeed { get; set; }
}