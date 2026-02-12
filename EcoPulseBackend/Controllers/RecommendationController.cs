using System.ComponentModel.DataAnnotations.Schema;
using EcoPulseBackend.Extensions;
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
            var recommendation = SelectRecommendation(model.UserLocation, tile);

            if (recommendation != null)
            {
                recommendations.Add(recommendation);
            }
        }

        return recommendations;
    }

    private RecommendationResult? SelectRecommendation(Point userLocation, TileModel tile)
    {
        var location = tile.Tile.Centroid;
        var recommendation = "нет рекомендации";

        var singleAvgConcentration = tile.SingleDangerZones.Count > 0 ? tile.SingleDangerZones.Average(s => s.AverageConcentration) : 0;
        var flowAvgConcentration = tile.VehicleFlowDangerZones.Count > 0 ? tile.VehicleFlowDangerZones.Average(s => s.AverageConcentration) : 0;
        var queueAvgConcentration = tile.VehicleQueueDangerZones.Count > 0 ? tile.VehicleQueueDangerZones.Average(s => s.AverageConcentration) : 0;

        var avgConcentration = singleAvgConcentration + flowAvgConcentration + queueAvgConcentration;
        
        Console.WriteLine(avgConcentration);
        
        if (avgConcentration < 225.5) 
        {
            return null;
        }
        
        if (singleAvgConcentration > flowAvgConcentration && singleAvgConcentration > queueAvgConcentration)
        {
            location = tile.SingleDangerZones.First().Polygon.Centroid;
            recommendation = "слишком большой выброс от котельной";
        }
        else if (flowAvgConcentration > singleAvgConcentration && flowAvgConcentration > queueAvgConcentration)
        {
            location = tile.VehicleFlowDangerZones.First().Points.GetPointN(tile.VehicleFlowDangerZones.First().Points.Count / 2);
            recommendation = "дорога перегружена";
        }
        else if (queueAvgConcentration > singleAvgConcentration && queueAvgConcentration > flowAvgConcentration)
        {
            location = tile.VehicleQueueDangerZones.First().Location;
            recommendation = "перекресток перегружен";
        }

        var distance = GeoUtils.Distance(userLocation, location);

        if (distance < 250)
        {
            recommendation = "закройте окна, большое скопление загрязнений от машин на соседнем перекрестке.";
        }
        
        // 1 воздержитесь от прогулок по этой улице в ближайшее время
        // 2 воздержитесь от прогулок рядом с этим перекрестком в ближайшее время 

        Console.WriteLine(avgConcentration);
        
        return new RecommendationResult
        {
            Location = location,
            AverageConcentration = avgConcentration,
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