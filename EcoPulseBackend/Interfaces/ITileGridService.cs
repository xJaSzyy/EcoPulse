using EcoPulseBackend.Models.DangerZone;
using EcoPulseBackend.Models.TileGrid;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Interfaces;

public interface ITileGridService
{
    List<TileModel> GenerateTileGrid(MultiPolygon cityPolygon, TileGridCalculateModel model);
    
    List<TileModel> GenerateTileArea(MultiPolygon mainPolygon, TileGridCalculateModel model);
}