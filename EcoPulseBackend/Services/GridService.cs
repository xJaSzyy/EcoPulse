using EcoPulseBackend.Extensions;
using EcoPulseBackend.Interfaces;
using EcoPulseBackend.Models.DangerZone;
using EcoPulseBackend.Models.TileGrid;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Union;

namespace EcoPulseBackend.Services;

public class GridService : IGridService
{
    private readonly GeometryFactory _geometryFactory = new();

    public List<TileModel> GenerateTileGrid(MultiPolygon mainPolygon, TileGridCalculateModel model)
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
                    var concentrations = GetConcentrations(model.SingleDangerZones, model.VehicleFlowDangerZones, model.TrafficLightQueueDangerZones, tilePolygon);

                    var color = DangerZoneUtils.GetColorByIndex(0);

                    if (concentrations.Count > 0)
                    {
                        color = DangerZoneUtils.GetColorByConcentration(concentrations.Average());
                    }

                    tiles.Add(new TileModel
                    {
                        Tile = tilePolygon,
                        Color = color,
                        AverageConcentration = concentrations.Count != 0 ? concentrations.Average() : -1
                    });
                }
            }
        }

        return MergeTilesByColor(tiles);
    }
    
    public List<TileModel> GenerateTileArea(MultiPolygon mainPolygon, TileGridCalculateModel model)
    {
        var tiles = new List<TileModel>();

        foreach (var polygon in mainPolygon)
        {
            var concentrations = GetConcentrations(model.SingleDangerZones, model.VehicleFlowDangerZones,
                model.TrafficLightQueueDangerZones, (polygon as Polygon)!);
            
            var color = DangerZoneUtils.GetColorByIndex(0);

            if (concentrations.Count > 0)
            {
                color = DangerZoneUtils.GetColorByConcentration(concentrations.Average());
            }

            tiles.Add(new TileModel
            {
                Tile = (polygon as Polygon)!,
                Color = color,
                AverageConcentration = concentrations.Count != 0 ? concentrations.Average() : -1
            });
        }

        return tiles;
    }

    private List<float> GetConcentrations(List<SingleDangerZone> singleZones, List<VehicleFlowDangerZone> flowZones, List<TrafficLightQueueDangerZone> queueZones, Polygon multiPolygon)
    {
        var intersectingSingleZones = singleZones.Where(x => x.Polygon.Intersects(multiPolygon)).ToList();
        var intersectingVehicleFlowZones = flowZones.Where(x => x.Points.Intersects(multiPolygon)).ToList();
        var intersectingTrafficLightQueueZones = queueZones.Where(x => x.Location.Intersects(multiPolygon)).ToList();

        return intersectingSingleZones
            .Select(s => s.AverageConcentration)
            .Concat(intersectingVehicleFlowZones.Select(f => f.AverageConcentration))
            .Concat(intersectingTrafficLightQueueZones.Select(q => q.AverageConcentration))
            .ToList();
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

    private List<TileModel> MergeTilesByColor(List<TileModel> tiles)
    {
        var result = new List<TileModel>();

        var groupedByColor = tiles.GroupBy(t => t.Color);

        foreach (var group in groupedByColor)
        {
            var geometries = group
                .Select(t => t.Tile)
                .Cast<Geometry>()
                .ToList();

            var unionGeometry = UnaryUnionOp.Union(geometries);

            switch (unionGeometry)
            {
                case Polygon polygon:
                    result.Add(CreateMergedTileModel(group, polygon));
                    break;
                case MultiPolygon multiPolygon:
                {
                    for (var i = 0; i < multiPolygon.NumGeometries; i++)
                    {
                        var poly = (Polygon)multiPolygon.GetGeometryN(i);
                        result.Add(CreateMergedTileModel(group, poly));
                    }

                    break;
                }
            }
        }

        return result;
    }

    private TileModel CreateMergedTileModel(
        IGrouping<string, TileModel> group,
        Polygon polygon)
    {
        var allConcentrations = group
            .Where(t => t.AverageConcentration >= 0)
            .Select(t => t.AverageConcentration)
            .ToList();

        return new TileModel
        {
            Tile = polygon,
            Color = group.Key,
            AverageConcentration = allConcentrations.Count != 0 ? allConcentrations.Average() : -1
        };
    }
}