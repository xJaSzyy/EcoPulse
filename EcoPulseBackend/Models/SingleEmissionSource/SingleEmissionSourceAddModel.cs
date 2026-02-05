using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoPulseBackend.Enums;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Models.SingleEmissionSource;

/// <summary>
/// Модель добавления источника выброса
/// </summary>
public class SingleEmissionSourceAddModel
{
    [Column(TypeName = "geometry(Point, 4326)")]
    public Point Location { get; set; } = null!;
    
    /// <summary>
    /// Температура выбрасываемой ГВС
    /// </summary>
    [Range(235, 265)]
    public float EjectedTemp { get; set; }

    /// <summary>
    /// Средняя скорость выхода ГВС из устья источника выброса, м/с
    /// </summary>
    [Range(15, 30)]
    public float AvgExitSpeed { get; set; }

    /// <summary>
    /// Высота источника выброса, м.
    /// </summary>
    [Range(13, 150)]
    public float HeightSource { get; set; }

    /// <summary>
    /// Диаметр устья источника, м.
    /// </summary>
    [Range(1, 10)]
    public float DiameterSource { get; set; }

    /// <summary>
    /// Коэффициент региона
    /// </summary>
    public CoefficientRegion TempStratificationRatio { get; set; }

    /// <summary>
    /// Коэффициент степени очистки
    /// </summary>
    public CoefficientDegreePurification SedimentationRateRatio { get; set; }
    
    /// <summary>
    /// Идентификатор города
    /// </summary>
    public int CityId { get; set; }
}