using EcoPulseBackend.Extensions;
using EcoPulseBackend.Interfaces;
using EcoPulseBackend.Models.DangerZone;
using EcoPulseBackend.Models.TileGrid;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Services;

public class TileGridService : ITileGridService
{
    private readonly GeometryFactory _geometryFactory = new();
    
    public List<TileModel> GenerateTileGrid(Polygon mainPolygon, List<SingleDangerZone> dangerZones, double tileSize)
    {
        var centerLat = mainPolygon.Centroid.Y; 
        var latStep = tileSize / 111000; 
        var lonStep = tileSize / (111000 * Math.Cos(centerLat * Math.PI / 180)); 
        
        var envelope = mainPolygon.EnvelopeInternal;
        var tiles = new List<TileModel>();

        for (var lon = Math.Floor(envelope.MinX / lonStep) * lonStep;
             lon < envelope.MaxX; 
             lon += lonStep)
        {
            for (var lat = Math.Floor(envelope.MinY / latStep) * latStep;
                 lat < envelope.MaxY; 
                 lat += latStep)
            {
                var tilePolygon = CreateTilePolygon(lon, lat, lonStep, latStep);

                if (mainPolygon.Intersects(tilePolygon))
                {
                    var intersectingDangers = dangerZones.Where(x => x.Polygon.Intersects(tilePolygon)).ToList();
                    var blendedColor = intersectingDangers.Count != 0
                        ? BlendColors(intersectingDangers.Select(d => d.AverageConcentration))
                        : DangerZoneUtils.GetColorByIndex(0);

                    tiles.Add(new TileModel
                    {
                        Tile = tilePolygon,
                        Color = blendedColor
                    });
                }
            }
        }
    
        return tiles;
    }
    
    private Polygon CreateTilePolygon(double minLon, double minLat, double lonSize, double latSize)
    {
        var coords = new[]
        {
            new Coordinate(minLon, minLat),
            new Coordinate(minLon + lonSize, minLat),
            new Coordinate(minLon + lonSize, minLat + latSize),
            new Coordinate(minLon, minLat + latSize),
            new Coordinate(minLon, minLat) 
        };
        
        var linearRing = _geometryFactory.CreateLinearRing(coords);
        return _geometryFactory.CreatePolygon(linearRing);
    }
    
    private static string BlendColors(IEnumerable<float> concentrations)
    {
        return DangerZoneUtils.GetColorByConcentration(concentrations.Average());
    }
}