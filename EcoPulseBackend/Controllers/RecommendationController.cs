using System.ComponentModel.DataAnnotations.Schema;
using EcoPulseBackend.Models.TileGrid;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Controllers;

[ApiController]
public class RecommendationController : ControllerBase
{
    [HttpPost("recommendation")]
    public IActionResult GetRecommendations(RecommendationGetModel model)
    {
        var recommendations = DefineRecommendations(model);

        return Ok(recommendations);
    }

    private List<RecommendationResult> DefineRecommendations(RecommendationGetModel model)
    {
        var recommendations = new List<RecommendationResult>();

        foreach (var tile in model.Tiles)
        {
            if (tile.AverageConcentration > 90)
            {
                var recommendation = SelectRecommendation(tile);

                recommendations.Add(recommendation);
            }
        }

        return recommendations;
    }

    private RecommendationResult SelectRecommendation(TileModel tile)
    {
        var location = tile.Tile.Centroid;
        var recommendation = "нет рекомендации";

        if (!tile.VehicleFlowDangerZones.Any() && !tile.VehicleQueueDangerZones.Any() && tile.SingleDangerZones.Any())
        {
            location = tile.SingleDangerZones.First().Polygon.Centroid;
            recommendation = "слишком большой выброс";
        }
        else if (tile.VehicleFlowDangerZones.Any() && !tile.VehicleQueueDangerZones.Any() && !tile.SingleDangerZones.Any())
        {
            location = tile.VehicleFlowDangerZones.First().Points.GetPointN(tile.VehicleFlowDangerZones.First().Points.Count / 2);
            recommendation = "дорога перегружена";
        }
        else if (tile.VehicleQueueDangerZones.Any() && !tile.SingleDangerZones.Any())
        {
            location = tile.VehicleQueueDangerZones.First().Location;
            recommendation = "перекресток перегружен";
        }
        
        // 1 постарайтесь обходить эту улицу в ближайшее время

        return new RecommendationResult
        {
            Location = location,
            AverageConcentration = tile.AverageConcentration,
            Recommendation = recommendation
        };
    }
}

public class RecommendationResult
{
    /// <summary>
    /// Координаты
    /// </summary>
    [Column(TypeName = "geometry(Point, 4326)")]
    public Point Location { get; set; } = null!;
    
    /// <summary>
    /// Среднее значение концентрации
    /// </summary>
    public float AverageConcentration { get; set; }

    /// <summary>
    /// Рекомендация
    /// </summary>
    public string Recommendation { get; set; } = null!;
}

public class RecommendationGetModel
{
    public int CityId { get; set; }
    
    public List<TileModel> Tiles { get; set; } = new();
    
    [Column(TypeName = "geometry(Point, 4326)")]
    public Point UserLocation { get; set; } = null!;
}