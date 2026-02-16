using EcoPulseBackend.Contexts;
using EcoPulseBackend.Models.City;
using EcoPulseBackend.Models.Weather;
using Microsoft.AspNetCore.Mvc;

namespace EcoPulseBackend.Controllers;

[ApiController]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public AdminController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    #region Weather

    [HttpGet("admin/weather")]
    public IActionResult GetWeather([FromQuery] int page, [FromQuery] int limit)
    {
        var total = _dbContext.Weathers.Count();
        var weathers = _dbContext.Weathers
            .OrderBy(w => w.Id)
            .Skip((page - 1) * limit)  
            .Take(limit)
            .ToList();

        var result = new AdminResultModel<Weather>
        {
            Data = weathers,
            Total = total,
            Page = page,
            Limit = limit,
            Pages = (int)Math.Ceiling((double)total / limit)
        };
        
        return Ok(result);
    }
    
    [HttpPost("admin/weather")]
    public async Task<IActionResult> AddWeather([FromBody] WeatherAddModel model)
    {
        var weather = new Weather
        {
            Date = model.Date,
            Temperature = model.Temperature,
            WindDirection = model.WindDirection,
            WindSpeed = model.WindSpeed,
            IconClass = model.IconClass
        };
        
        _dbContext.Weathers.Add(weather);
        await _dbContext.SaveChangesAsync();

        return Ok(weather);
    }
    
    [HttpPut("admin/weather")]
    public async Task<IActionResult> UpdateWeather([FromBody] WeatherUpdateModel model)
    {
        var weather = _dbContext.Weathers.FirstOrDefault(w => w.Id == model.Id);

        if (weather == null)
        {
            return NotFound();
        }

        weather.Date = model.Date ?? weather.Date;
        weather.Temperature = model.Temperature ?? weather.Temperature;
        weather.WindDirection = model.WindDirection ?? weather.WindDirection;
        weather.WindSpeed = model.WindSpeed ?? weather.WindSpeed;
        weather.IconClass = model.IconClass ?? weather.IconClass;
        
        _dbContext.Weathers.Update(weather);
        await _dbContext.SaveChangesAsync();

        return Ok(weather);
    }
    
    [HttpDelete("admin/weather/{id}")]
    public async Task<IActionResult> DeleteWeather(int id)
    {
        var weather = _dbContext.Weathers.FirstOrDefault(w => w.Id == id);

        if (weather == null)
        {
            return NotFound();
        }
        
        _dbContext.Weathers.Remove(weather);
        await _dbContext.SaveChangesAsync();

        return Ok(weather);
    }

    #endregion
    
    #region City

    [HttpGet("admin/city")]
    public IActionResult GetCity([FromQuery] int page, [FromQuery] int limit)
    {
        var total = _dbContext.Cities.Count();
        var cities = _dbContext.Cities
            .OrderBy(w => w.Id)
            .Skip((page - 1) * limit)  
            .Take(limit)
            .ToList();

        var result = new AdminResultModel<City>
        {
            Data = cities,
            Total = total,
            Page = page,
            Limit = limit,
            Pages = (int)Math.Ceiling((double)total / limit)
        };
        
        return Ok(result);
    }
    
    [HttpPost("admin/city")]
    public async Task<IActionResult> AddCity([FromBody] CityAddModel model)
    {
        var city = new City
        {
            Name = model.Name,
            Location = model.Location,
            Polygon = model.Polygon
        };
        
        _dbContext.Cities.Add(city);
        await _dbContext.SaveChangesAsync();

        return Ok(city);
    }
    
    [HttpPut("admin/city")]
    public async Task<IActionResult> UpdateCity([FromBody] CityUpdateModel model)
    {
        var city = _dbContext.Cities.FirstOrDefault(w => w.Id == model.Id);

        if (city == null)
        {
            return NotFound();
        }

        city.Name = model.Name ?? city.Name;
        city.Location = model.Location ?? city.Location;
        city.Polygon = model.Polygon ?? city.Polygon;
        
        _dbContext.Cities.Update(city);
        await _dbContext.SaveChangesAsync();

        return Ok(city);
    }
    
    [HttpDelete("admin/city/{id}")]
    public async Task<IActionResult> DeleteCity(int id)
    {
        var city = _dbContext.Cities.FirstOrDefault(w => w.Id == id);

        if (city == null)
        {
            return NotFound();
        }
        
        _dbContext.Cities.Remove(city);
        await _dbContext.SaveChangesAsync();

        return Ok(city);
    }

    #endregion
}

public class AdminResultModel<T>
{
    public List<T> Data { get; set; } = [];
    
    public int Total { get; set; }
    
    public int Page { get; set; }
    
    public int Limit { get; set; }
    
    public int Pages { get; set; }
}

public class WeatherAddModel
{
    public DateTimeOffset Date { get; set; }
    
    public float Temperature { get; set; }
    
    public int WindDirection { get; set; }
    
    public float WindSpeed { get; set; }
    
    public string IconClass { get; set; } = null!;
}

public class WeatherUpdateModel
{
    public int Id { get; set; }
    
    public DateTimeOffset? Date { get; set; }
    
    public float? Temperature { get; set; }
    
    public int? WindDirection { get; set; }
    
    public float? WindSpeed { get; set; }
    
    public string? IconClass { get; set; } = null!;
}