using EcoPulseBackend.Contexts;
using EcoPulseBackend.Models.City;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Controllers;

[ApiController]
public class CityController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public CityController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("city/{id:int}")]
    public IActionResult GetCityById(int id)
    {
        var city = _dbContext.Cities.FirstOrDefault(c => c.Id == id);

        if (city == null)
        {
            return NotFound();
        }

        return Ok(city);
    }

    [HttpPost("city")]
    public async Task<IActionResult> CreateCity([FromBody] CityAddModel model)
    {
        var city = new City
        {
            Location = model.Location,
            Name = model.Name
        };
        
        _dbContext.Cities.Add(city);
        await _dbContext.SaveChangesAsync();
        
        return Ok(city);
    }
}