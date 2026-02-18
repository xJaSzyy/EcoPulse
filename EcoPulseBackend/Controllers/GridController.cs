using EcoPulseBackend.Contexts;
using EcoPulseBackend.Interfaces;
using EcoPulseBackend.Models.TileGrid;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;

[ApiController]
public class GridController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IGridService _gridService;

    public GridController(ApplicationDbContext dbContext, IGridService gridService)
    {
        _dbContext = dbContext;
        _gridService = gridService;
    }

    [HttpPost("grid/tile")]
    public IActionResult CalculateTileGrid([FromBody] TileGridCalculateModel model)
    {
        var cities = _dbContext.Cities
            .Where(c => model.CityIds.Contains(c.Id))
            .ToList();

        var results = new List<TileGridResult>();

        foreach (var city in cities)
        {
            if (city.Polygon == null)
            {
                continue;
            }
            
            var tiles = _gridService.GenerateTileGrid(city.Polygon, model);
            results.Add(new TileGridResult
            {
                CityId = city.Id,
                Tiles = tiles
            });
        }

        return Ok(results);
    }

    [HttpPost("grid/area")]
    public IActionResult CalculateTileArea([FromBody] TileGridCalculateModel model)
    {
        var cities = _dbContext.Cities
            .Where(c => model.CityIds.Contains(c.Id))
            .ToList();

        var results = new List<TileGridResult>();

        foreach (var city in cities)
        {
            if (city.Polygon == null)
            {
                continue;
            }
            
            var tiles = _gridService.GenerateTileArea(city.Polygon, model);
            results.Add(new TileGridResult
            {
                CityId = city.Id,
                Tiles = tiles
            });
        }

        return Ok(results);
    }
}