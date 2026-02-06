using System.ComponentModel.DataAnnotations.Schema;
using EcoPulseBackend.Contexts;
using EcoPulseBackend.Extensions;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;

[ApiController]
public class TileGridController : ControllerBase
{
    private readonly GeometryFactory _geometryFactory = new();
    private readonly ApplicationDbContext _dbContext;

    public TileGridController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("tile-grid/city/{id}")]
    public IActionResult GetTileGridByCityId(int id)
    {
        var city = _dbContext.Cities.FirstOrDefault(c => c.Id == id);
        
        if (city == null)
        {
            return NotFound();
        }

        double tileSizeMeters = 500;
        var centerLat = city.Polygon.Centroid.Y; 
        var latStep = tileSizeMeters / 111000; 
        var lonStep = tileSizeMeters / (111000 * Math.Cos(centerLat * Math.PI / 180)); 
        
        var tiles = GenerateTileGridWithSteps(city.Polygon, latStep, lonStep);
        return Ok(tiles);
    }

    private List<TileModel> GenerateTileGridWithSteps(Polygon mainPolygon, double latStep, double lonStep)
    {
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
                    Random rnd = new();
                    tiles.Add(new TileModel
                    {
                        Tile = tilePolygon,
                        Color = DangerZoneUtils.GetColorByConcentration(rnd.Next(1, 256))
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
}

public class TileModel
{
    [Column(TypeName = "geometry(Polygon, 4326)")]
    public Polygon Tile { get; set; } = null!;

    public string Color { get; set; } = null!;
}