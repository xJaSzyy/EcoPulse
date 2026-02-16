using EcoPulseBackend.Extensions;
using EcoPulseBackend.Interfaces;
using EcoPulseBackend.Models.TileGrid;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Services;

public class TileGridService : ITileGridService
{
    private readonly GeometryFactory _geometryFactory = new();
    
    public List<TileModel> GenerateTileGrid(Polygon mainPolygon, TileGridCalculateModel model)
    {
        var centerLat = mainPolygon.Centroid.Y; 
        var latStep = model.TileSize / 111000; 
        var lonStep = model.TileSize / (111000 * Math.Cos(centerLat * Math.PI / 180)); 
        
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
                    var intersectingSingleZones = model.SingleDangerZones.Where(x => x.Polygon.Intersects(tilePolygon)).ToList();
                    var intersectingVehicleFlowZones = model.VehicleFlowDangerZones.Where(x => x.Points.Intersects(tilePolygon)).ToList();
                    var intersectingTrafficLightQueueZones = model.TrafficLightQueueDangerZones.Where(x => x.Location.Intersects(tilePolygon)).ToList();

                    var concentrations = intersectingSingleZones
                        .Select(s => s.AverageConcentration)
                        .Concat(intersectingVehicleFlowZones.Select(f => f.AverageConcentration))
                        .Concat(intersectingTrafficLightQueueZones.Select(q => q.AverageConcentration))
                        .ToList();
                    
                    var blendedColor = BlendColors(concentrations);

                    if (concentrations.Count == 0)
                    {
                        continue;
                    }
                    
                    tiles.Add(new TileModel
                    {
                        Tile = tilePolygon,
                        Color = blendedColor,
                        AverageConcentration = concentrations.Count != 0 ? (float)Math.Round(concentrations.Average(), 1) : -1,
                        SingleDangerZones = intersectingSingleZones,
                        VehicleFlowDangerZones = intersectingVehicleFlowZones,
                        VehicleQueueDangerZones = intersectingTrafficLightQueueZones
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
    
    private static string BlendColors(List<float> concentrations)
    {
        return concentrations.Count == 0 ? DangerZoneUtils.GetColorByIndex(0) : DangerZoneUtils.GetColorByConcentration(concentrations.Average());
    }
}