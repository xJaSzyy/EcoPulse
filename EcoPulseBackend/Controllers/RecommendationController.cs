using EcoPulseBackend.Extensions;
using EcoPulseBackend.Models.Recommendation;
using EcoPulseBackend.Models.TileGrid;
using Microsoft.AspNetCore.Mvc;

namespace EcoPulseBackend.Controllers;

[ApiController]
public class RecommendationController : ControllerBase
{
    [HttpPost("recommendation")]
    public IActionResult GetRecommendations(RecommendationGetModel model)
    {
        var recommendation = SelectRecommendation(model);

        return Ok(recommendation);
    }

    private RecommendationResult SelectRecommendation(RecommendationGetModel model)
    {
        TileModel? userTile = null;

        foreach (var tile in model.Tiles.Where(tile => tile.Tile.Intersects(model.UserLocation)))
        {
            userTile = tile;
            break;
        }

        if (userTile == null)
        {
            return new RecommendationResult();
        }

        var dangerIndex = DangerZoneUtils.GetIndexByConcentration(userTile.AverageConcentration);
        List<string> recommendations = dangerIndex switch
        {
            0 => ["Воздух чистый", "выходите гулять в ближайшее время", "смело открывайте окна"],
            1 => ["Воздух почти чистый", "выходите гулять", "рекомендуем проветрить помещение"],
            2 =>
            [
                "Воздух почти грязный", "чувствительным людям следует быть осторожными",
                "чувствительным людям лучше сидеть дома"
            ],
            3 => ["Воздух грязный", "не стоит долго находиться на улице", "не рекомендуем открывать окна"],
            4 => ["Воздух слишком грязный", "воздержитесь от прогулок", "не открывайте окна"],
            _ => ["Воздух чересчур грязный", "не выходите из помещения"]
        };

        var rnd = new Random();

        return new RecommendationResult
        {
            RecommendationLevel = recommendations[0],
            RecommendationText = recommendations[rnd.Next(1, recommendations.Count)]
        };
    }
}