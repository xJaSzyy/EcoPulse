using EcoPulseBackend.Models.DangerZone;
using EcoPulseBackend.Models.TileGrid;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Interfaces;

public interface IGridService
{
    List<TileModel> GenerateTileGrid(MultiPolygon cityPolygon, TileGridCalculateModel model);
    
    List<TileModel> GenerateTileArea(MultiPolygon mainPolygon, TileGridCalculateModel model);
}