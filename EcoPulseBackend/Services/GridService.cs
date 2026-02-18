using EcoPulseBackend.Extensions;
using EcoPulseBackend.Interfaces;
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
                    var intersectingSingleZones = model.SingleDangerZones.Where(x => x.Polygon.Intersects(tilePolygon)).ToList();
                    var intersectingVehicleFlowZones = model.VehicleFlowDangerZones.Where(x => x.Points.Intersects(tilePolygon)).ToList();
                    var intersectingTrafficLightQueueZones = model.TrafficLightQueueDangerZones.Where(x => x.Location.Intersects(tilePolygon)).ToList();

                    var concentrations = intersectingSingleZones
                        .Select(s => s.AverageConcentration)
                        .Concat(intersectingVehicleFlowZones.Select(f => f.AverageConcentration))
                        .Concat(intersectingTrafficLightQueueZones.Select(q => q.AverageConcentration))
                        .ToList();
                    
                    var blendedColor = BlendColors(concentrations);

                    /*if (concentrations.Count == 0)
                    {
                        continue;
                    }*/
                    
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
    
        return MergeTilesByColor(tiles);
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

            var unionedGeometry = UnaryUnionOp.Union(geometries);

            if (unionedGeometry is Polygon polygon)
            {
                result.Add(CreateMergedTileModel(group, polygon));
            }
            else if (unionedGeometry is MultiPolygon multiPolygon)
            {
                for (int i = 0; i < multiPolygon.NumGeometries; i++)
                {
                    var poly = (Polygon)multiPolygon.GetGeometryN(i);
                    result.Add(CreateMergedTileModel(group, poly));
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
            AverageConcentration = allConcentrations.Count != 0 
                ? (float)Math.Round(allConcentrations.Average(), 1) 
                : -1,
            SingleDangerZones = group.SelectMany(t => t.SingleDangerZones).ToList(),
            VehicleFlowDangerZones = group.SelectMany(t => t.VehicleFlowDangerZones).ToList(),
            VehicleQueueDangerZones = group.SelectMany(t => t.VehicleQueueDangerZones).ToList()
        };
    }

    
    public List<TileModel> GenerateTileArea(MultiPolygon mainPolygon, TileGridCalculateModel model)
    {
        var tiles = new List<TileModel>();

        foreach (var polygon in mainPolygon)
        {
            var intersectingSingleZones = model.SingleDangerZones.Where(x => x.Polygon.Intersects(polygon)).ToList();
            var intersectingVehicleFlowZones = model.VehicleFlowDangerZones.Where(x => x.Points.Intersects(polygon)).ToList();
            var intersectingTrafficLightQueueZones = model.TrafficLightQueueDangerZones.Where(x => x.Location.Intersects(polygon)).ToList();

            var concentrations = intersectingSingleZones
                .Select(s => s.AverageConcentration)
                .Concat(intersectingVehicleFlowZones.Select(f => f.AverageConcentration))
                .Concat(intersectingTrafficLightQueueZones.Select(q => q.AverageConcentration))
                .ToList();
                    
            var blendedColor = BlendColors(concentrations);
            
            tiles.Add(new TileModel
            {
                Tile = (polygon as Polygon)!,
                Color = blendedColor,
                AverageConcentration = concentrations.Count != 0 ? (float)Math.Round(concentrations.Average(), 1) : -1,
                SingleDangerZones = intersectingSingleZones,
                VehicleFlowDangerZones = intersectingVehicleFlowZones,
                VehicleQueueDangerZones = intersectingTrafficLightQueueZones
            });
        }

        return tiles;
    }
}