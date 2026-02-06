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
    public IActionResult GetTileGridByCityId(int id, [FromQuery] double tileSize = 1000)
    {
        var city = _dbContext.Cities.FirstOrDefault(c => c.Id == id);
        
        if (city == null)
        {
            return NotFound();
        }

        var centerLat = city.Polygon.Centroid.Y; 
        var latStep = tileSize / 111000; 
        var lonStep = tileSize / (111000 * Math.Cos(centerLat * Math.PI / 180)); 
        
        var tiles = GenerateTileGridWithSteps(city.Polygon, latStep, lonStep);
        return Ok(tiles);
    }
    
    [HttpPost("tile-grid/city/{id}/danger-overlay")]
    public IActionResult GetTileGridWithDangerOverlay(int id, [FromBody] Polygon dangerZone, [FromQuery] double tileSize = 1000)
    {
        var city = _dbContext.Cities.FirstOrDefault(c => c.Id == id);
    
        if (city == null)
        {
            return NotFound();
        }

        var centerLat = city.Polygon.Centroid.Y; 
        var latStep = tileSize / 111000; 
        var lonStep = tileSize / (111000 * Math.Cos(centerLat * Math.PI / 180)); 
    
        var tiles = GenerateTileGridWithDangerOverlay(city.Polygon, dangerZone, latStep, lonStep);
        return Ok(tiles);
    }

    private List<TileModel> GenerateTileGridWithDangerOverlay(Polygon mainPolygon, Polygon dangerZone, 
        double latStep, double lonStep)
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
                    var intersectsDanger = dangerZone.Intersects(tilePolygon);
                    tiles.Add(new TileModel
                    {
                        Tile = tilePolygon,
                        Color = intersectsDanger ? "rgba(255, 0, 0, 0.7)" : "rgb(255, 255, 255)"
                    });
                }
            }
        }
    
        return tiles;
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
                    tiles.Add(new TileModel
                    {
                        Tile = tilePolygon,
                        Color = "rgb(255, 255, 255)"
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