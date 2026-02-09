using EcoPulseBackend.Contexts;
using EcoPulseBackend.Interfaces;
using EcoPulseBackend.Models.DangerZone;
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
    
    [HttpPost("tile-grid/city/{id}")]
    public IActionResult GetTileGridWithDangerOverlay(int id, [FromBody] List<SingleDangerZone> dangerZones, [FromQuery] double tileSize = 1000)
    {
        var city = _dbContext.Cities.FirstOrDefault(c => c.Id == id);
    
        if (city == null)
        {
            return NotFound();
        }
    
        var tiles = _tileGridService.GenerateTileGrid(city.Polygon, dangerZones, tileSize);
        
        return Ok(tiles);
    }
}