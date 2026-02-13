using System.ComponentModel.DataAnnotations.Schema;
using EcoPulseBackend.Models.TileGrid;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Models.Recommendation;

public class RecommendationGetModel
{
    public List<TileModel> Tiles { get; set; } = [];
    
    [Column(TypeName = "geometry(Point, 4326)")]
    public Point UserLocation { get; set; } = null!;
}