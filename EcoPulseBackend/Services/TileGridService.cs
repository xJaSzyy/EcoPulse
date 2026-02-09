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
                        ? BlendColors(intersectingDangers.Select(d => d.Color))
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

    private static (int r, int g, int b) ParseRgb(string? color)
    {
        if (string.IsNullOrWhiteSpace(color)) return (255, 255, 255);
    
        var match = System.Text.RegularExpressions.Regex.Match(color, @"rgba?\((\d+),\s*(\d+),\s*(\d+)(?:,\s*[\d.]+)?\)");
        if (!match.Success)
        {
            return (255, 255, 255);
        }
    
        return (
            int.Parse(match.Groups[1].Value),
            int.Parse(match.Groups[2].Value),
            int.Parse(match.Groups[3].Value)
        );
    }

    private static string BlendColors(IEnumerable<string> colors)
    {
        var parsed = colors.Select(ParseRgb).ToArray();
    
        var avgR = Math.Clamp(parsed.Sum(c => c.r) / parsed.Length, 0, 255);
        var avgG = Math.Clamp(parsed.Sum(c => c.g) / parsed.Length, 0, 255);
        var avgB = Math.Clamp(parsed.Sum(c => c.b) / parsed.Length, 0, 255);
    
        return $"rgb({avgR}, {avgG}, {avgB})";
    }
}