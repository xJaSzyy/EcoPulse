using EcoPulseBackend.Contexts;
using EcoPulseBackend.Interfaces;
using EcoPulseBackend.Models.TileGrid;
using Microsoft.AspNetCore.Mvc;

[ApiController]
public class TileGridController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITileGridService _tileGridService;

    public TileGridController(ApplicationDbContext dbContext, ITileGridService tileGridService)
    {
        _dbContext = dbContext;
        _tileGridService = tileGridService;
    }
    
    [HttpPost("tile-grid/calculate")]
    public IActionResult CalculateTileGrid([FromBody] TileGridCalculateModel model)
    {
        var city = _dbContext.Cities.FirstOrDefault(c => c.Id == model.CityId);
    
        if (city == null)
        {
            return NotFound();
        }
        
        var tiles = _tileGridService.GenerateTileGrid(city.Polygon, model);
        
        return Ok(tiles);
    }
}