using System.ComponentModel.DataAnnotations.Schema;
using EcoPulseBackend.Models.TileGrid;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Controllers;

[ApiController]
public class RecommendationController : ControllerBase
{
    [HttpPost("recommendation")]
    public IActionResult GetRecommendations(TileGridResult model)
    {
        var recommendations = DefineRecommendations(model);
        
        return Ok(recommendations);
    }

    private List<RecommendationResult> DefineRecommendations(TileGridResult model)
    {
        var recommendations = new List<RecommendationResult>();
    
        foreach (var tile in model.Tiles)
        {
            if (tile.AverageConcentration > 300)
            {
                recommendations.Add(new RecommendationResult
                {
                    Location = tile.Tile.Centroid,
                    AverageConcentration = tile.AverageConcentration,
                    Recommendation = "Поставить фильтр"
                });
            }
        }
    
        return recommendations;
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