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

    private RecommendationResult DefineRecommendations(RecommendationGetModel model)
    {
        var recommendation = new RecommendationResult();

        foreach (var tile in model.Tiles)
        {
            SelectRecommendation(recommendation, model.UserLocation, tile);
        }

        return recommendation;
    }

    private void SelectRecommendation(RecommendationResult result, Point userLocation, TileModel tile)
    {
        var location = tile.Tile.Centroid;
        var recommendation = string.Empty;
        var description = string.Empty;

        var singleAvgConcentration = tile.SingleDangerZones.Count > 0 ? tile.SingleDangerZones.Average(s => s.AverageConcentration) : 0;
        var flowAvgConcentration = tile.VehicleFlowDangerZones.Count > 0 ? tile.VehicleFlowDangerZones.Average(s => s.AverageConcentration) : 0;
        var queueAvgConcentration = tile.VehicleQueueDangerZones.Count > 0 ? tile.VehicleQueueDangerZones.Average(s => s.AverageConcentration) : 0;

        var avgConcentration = singleAvgConcentration + flowAvgConcentration + queueAvgConcentration;
        
        if (avgConcentration < 225.5) 
        {
            return;
        }
        
        if (singleAvgConcentration > flowAvgConcentration && singleAvgConcentration > queueAvgConcentration)
        {
            location = tile.SingleDangerZones.First().Polygon.Centroid;
            description = "слишком большой выброс от котельной";
        }
        else if (flowAvgConcentration > singleAvgConcentration && flowAvgConcentration > queueAvgConcentration)
        {
            var rnd = new Random();
            var flowZoneIndex = rnd.Next(0, tile.VehicleFlowDangerZones.Count);
            var flowPointIndex = rnd.Next(0, 3);
            location = flowPointIndex switch
            {
                0 => tile.VehicleFlowDangerZones[flowZoneIndex].Points.StartPoint,
                1 => tile.VehicleFlowDangerZones[flowZoneIndex].Points.Centroid,
                _ => tile.VehicleFlowDangerZones[flowZoneIndex].Points.EndPoint
            };

            if (singleAvgConcentration > 35.5)
            {
                description = "дорога перегружена, небольшой выброс от котельной";
                if (!tile.SingleDangerZones.Any(s => s.Polygon.Intersects(location)))
                {
                    location = tile.SingleDangerZones.First().Location;
                }
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
        
        result.HotSpots.Add(new HotSpot
        {
            Location = location,
            AverageConcentration = avgConcentration,
            Description = description
        });

        if (recommendation != string.Empty)
        {
            result.Recommendation = recommendation;
        }
    }
}

public class RecommendationResult
{
    /// <summary>
    /// Рекомендация
    /// </summary>
    public string? Recommendation { get; set; }

    /// <summary>
    /// Точки скопления загрязнений
    /// </summary>
    public List<HotSpot> HotSpots { get; set; } = [];
}

public class RecommendationGetModel
{
    public int CityId { get; set; }
    
    public List<TileModel> Tiles { get; set; } = [];
    
    [Column(TypeName = "geometry(Point, 4326)")]
    public Point UserLocation { get; set; } = null!;
}

public class HotSpot
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
    /// Причина скопления загрязнений
    /// </summary>
    public string Description { get; set; } = null!;
}