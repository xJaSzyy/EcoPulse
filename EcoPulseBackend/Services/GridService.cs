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

        var preparedMainPolygon =
            NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory
                .Prepare(mainPolygon);

        var singleIndex = new NetTopologySuite.Index.Strtree.STRtree<SingleDangerZone>();
        foreach (var zone in model.SingleDangerZones)
            singleIndex.Insert(zone.Polygon.EnvelopeInternal, zone);

        var flowIndex = new NetTopologySuite.Index.Strtree.STRtree<VehicleFlowDangerZone>();
        foreach (var zone in model.VehicleFlowDangerZones)
            flowIndex.Insert(zone.Points.EnvelopeInternal, zone);

        var queueIndex = new NetTopologySuite.Index.Strtree.STRtree<TrafficLightQueueDangerZone>();
        foreach (var zone in model.TrafficLightQueueDangerZones)
            queueIndex.Insert(zone.Location.EnvelopeInternal, zone);

        var preparedDistricts =
            new List<(Polygon polygon, NetTopologySuite.Geometries.Prepared.IPreparedGeometry prepared, float avg)>();

        for (var i = 0; i < mainPolygon.NumGeometries; i++)
        {
            var district = (Polygon)mainPolygon.GetGeometryN(i);

            var prepared = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory
                .Prepare(district);

            var avgDistrict = GetAverageConcentration(
                singleIndex,
                flowIndex,
                queueIndex,
                district);

            preparedDistricts.Add((district, prepared, avgDistrict));
        }

        for (var lon = Math.Floor(envelope.MinX / lonStep) * lonStep; lon < envelope.MaxX; lon += lonStep)
        {
            for (var lat = Math.Floor(envelope.MinY / latStep) * latStep; lat < envelope.MaxY; lat += latStep)
            {
                var tilePolygon = CreateTilePolygon(lon, lat, lonStep, latStep);

                if (!preparedMainPolygon.Intersects(tilePolygon))
                    continue;

                var avg = GetAverageConcentration(
                    singleIndex,
                    flowIndex,
                    queueIndex,
                    tilePolygon);

                if (avg < 0)
                {
                    foreach (var district in preparedDistricts)
                    {
                        if (district.prepared.Intersects(tilePolygon))
                        {
                            avg = district.avg;
                            break;
                        }
                    }
                }

                var color = avg >= 0
                    ? DangerZoneUtils.GetColorByConcentration(avg)
                    : DangerZoneUtils.GetColorByIndex(0);

                tiles.Add(new TileModel
                {
                    Tile = tilePolygon,
                    Color = color,
                    AverageConcentration = avg
                });
            }
        }

        return MergeTilesByColor(tiles);
    }

    public List<TileModel> GenerateTileArea(MultiPolygon mainPolygon, TileGridCalculateModel model)
    {
        var tiles = new List<TileModel>();

        var singleIndex = new NetTopologySuite.Index.Strtree.STRtree<SingleDangerZone>();
        foreach (var zone in model.SingleDangerZones)
            singleIndex.Insert(zone.Polygon.EnvelopeInternal, zone);

        var flowIndex = new NetTopologySuite.Index.Strtree.STRtree<VehicleFlowDangerZone>();
        foreach (var zone in model.VehicleFlowDangerZones)
            flowIndex.Insert(zone.Points.EnvelopeInternal, zone);

        var queueIndex = new NetTopologySuite.Index.Strtree.STRtree<TrafficLightQueueDangerZone>();
        foreach (var zone in model.TrafficLightQueueDangerZones)
            queueIndex.Insert(zone.Location.EnvelopeInternal, zone);
        
        foreach (var area in mainPolygon)
        {
            var avgConcentration = GetAverageConcentration(singleIndex, flowIndex, queueIndex, (area as Polygon)!);
            var color = DangerZoneUtils.GetColorByConcentration(avgConcentration);

            tiles.Add(new TileModel
            {
                Tile = (area as Polygon)!,
                Color = color,
                AverageConcentration = avgConcentration
            });
        }

        return tiles;
    }

    private float GetAverageConcentration(
        NetTopologySuite.Index.Strtree.STRtree<SingleDangerZone> singleIndex,
        NetTopologySuite.Index.Strtree.STRtree<VehicleFlowDangerZone> flowIndex,
        NetTopologySuite.Index.Strtree.STRtree<TrafficLightQueueDangerZone> queueIndex,
        Polygon polygon)
    {
        float sum = 0;
        var count = 0;

        var singleCandidates = singleIndex.Query(polygon.EnvelopeInternal);
        foreach (var zone in singleCandidates)
        {
            if (zone.Polygon.Intersects(polygon))
            {
                sum += zone.AverageConcentration;
                count++;
            }
        }

        var flowCandidates = flowIndex.Query(polygon.EnvelopeInternal);
        foreach (var zone in flowCandidates)
        {
            if (zone.Points.Intersects(polygon))
            {
                sum += zone.AverageConcentration;
                count++;
            }
        }

        var queueCandidates = queueIndex.Query(polygon.EnvelopeInternal);
        foreach (var zone in queueCandidates)
        {
            if (zone.Location.Intersects(polygon))
            {
                sum += zone.AverageConcentration;
                count++;
            }
        }

        return count == 0 ? -1 : sum / count;
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

        var grouped = tiles.GroupBy(t => t.AverageConcentration);

        foreach (var group in grouped)
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
        IGrouping<float, TileModel> group,
        Polygon polygon)
    {
        var allConcentrations = group
            .Where(t => t.AverageConcentration >= 0)
            .Select(t => t.AverageConcentration)
            .ToList();

        return new TileModel
        {
            Tile = polygon,
            Color = DangerZoneUtils.GetColorByConcentration(group.Key),
            AverageConcentration = group.Key
        };
    }
}