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
        var description = "";

        var singleAvgConcentration = tile.SingleDangerZones.Count > 0 ? tile.SingleDangerZones.Average(s => s.AverageConcentration) : 0;
        var flowAvgConcentration = tile.VehicleFlowDangerZones.Count > 0 ? tile.VehicleFlowDangerZones.Average(s => s.AverageConcentration) : 0;
        var queueAvgConcentration = tile.VehicleQueueDangerZones.Count > 0 ? tile.VehicleQueueDangerZones.Average(s => s.AverageConcentration) : 0;

        var avgConcentration = singleAvgConcentration + flowAvgConcentration + queueAvgConcentration;
        
        if (avgConcentration < 225.5) 
        {
            return null;
        }
        
        if (singleAvgConcentration > flowAvgConcentration && singleAvgConcentration > queueAvgConcentration)
        {
            location = tile.SingleDangerZones.First().Polygon.Centroid;
            description = "слишком большой выброс от котельной";
        }
        else if (flowAvgConcentration > singleAvgConcentration && flowAvgConcentration > queueAvgConcentration)
        {
            location = tile.VehicleFlowDangerZones.First().Points.GetPointN(tile.VehicleFlowDangerZones.First().Points.Count / 2);
            
            //TODO переделать location у дорог
            if (singleAvgConcentration > 35.5)
            {
                description = "дорога перегружена, небольшой выброс от котельной";
            }
            else if (queueAvgConcentration > 35.5)
            {
                description = "дорога и перекресток перегружены";
            }
            else
            {
                description = "дорога перегружена";
            }
        }
        else if (queueAvgConcentration > singleAvgConcentration && queueAvgConcentration > flowAvgConcentration)
        {
            location = tile.VehicleQueueDangerZones.First().Location;
            if (singleAvgConcentration > 35.5)
            {
                description = "перекресток перегружен, небольшой выброс от котельной";
            }
            else if (flowAvgConcentration > 35.5)
            {
                description = "перекресток и дорога перегружены";
            }
            else
            {
                description = "перекресток перегружен";
            }
        }

        var distance = GeoUtils.Distance(userLocation, location);

        if (distance < 250)
        {
            recommendation = "закройте окна";
            
            if (description == "дорога перегружена")
            {
                description = "большое скопление загрязнений от машин на соседней улице";
            }
            else if (description == "перекресток перегружен")
            {
                description = "большое скопление загрязнений от машин на соседнем перекрестке";
            }
            else if (description == "слишком большой выброс от котельной")
            {
                description = "вблизи большое скопление загрязнений от котельной";
            }
        } 
        else if (distance < 500)
        {
            recommendation = "воздержитесь от прогулок";
            
            if (description == "дорога перегружена")
            {
                description = "большое скопление загрязнений от машин на ближайших улицах";
            }
            else if (description == "перекресток перегружен")
            {
                description = "большое скопление загрязнений от машин на ближайших перекрестках";
            }
            else if (description == "слишком большой выброс от котельной")
            {
                description = "большое скопление загрязнений от котельной";
            }
        }
        
        return new RecommendationResult
        {
            Location = location,
            AverageConcentration = avgConcentration,
            Recommendation = recommendation,
            Description = description
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

    public string Description { get; set; } = null!;
}

public class RecommendationGetModel
{
    public int CityId { get; set; }
    
    public List<TileModel> Tiles { get; set; } = new();
    
    [Column(TypeName = "geometry(Point, 4326)")]
    public Point UserLocation { get; set; } = null!;
}