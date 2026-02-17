using EcoPulseBackend.Contexts;
using EcoPulseBackend.Interfaces;
using EcoPulseBackend.Models.TileGrid;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;

[ApiController]
public class GridController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITileGridService _tileGridService;

    public GridController(ApplicationDbContext dbContext, ITileGridService tileGridService)
    {
        _dbContext = dbContext;
        _tileGridService = tileGridService;
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
            var tiles = _tileGridService.GenerateTileGrid(city.Polygon, model);
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
            var tiles = _tileGridService.GenerateTileArea(city.Polygon, model);
            results.Add(new TileGridResult
            {
                CityId = city.Id,
                Tiles = tiles
            });
        }

        return Ok(results);
    }
}