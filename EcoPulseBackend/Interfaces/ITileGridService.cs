using EcoPulseBackend.Models.DangerZone;
using EcoPulseBackend.Models.TileGrid;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Interfaces;

public interface ITileGridService
{
    List<TileModel> GenerateTileGrid(Polygon cityPolygon, TileGridCalculateModel model);
    
    List<TileModel> GenerateTileArea(Polygon cityPolygon, TileGridCalculateModel model);
}