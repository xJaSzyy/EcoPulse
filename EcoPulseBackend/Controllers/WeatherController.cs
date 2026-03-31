using System.Net;
using EcoPulseBackend.Contexts;
using EcoPulseBackend.Models.Weather;
using Microsoft.AspNetCore.Mvc;

namespace EcoPulseBackend.Controllers;

[ApiController]
public class WeatherController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public WeatherController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("weather/save")]
    [ProducesResponseType(typeof(WeatherViewModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> SaveWeather([FromBody] WeatherViewModel model)
    {
        _dbContext.Weathers.Add(new Weather
        {
            Date = model.Date,
            Temperature = model.Temperature,
            WindSpeed = model.WindSpeed,
            WindDirection = model.WindDirection,
            IconClass = model.IconClass
        });
        await _dbContext.SaveChangesAsync();

        return Ok(model);
    }

    [HttpGet("weather/current")]
    [ProducesResponseType(typeof(WeatherViewModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetCurrentWeather([FromQuery] string city)
    {
        var weather = _dbContext.Weathers
            .OrderByDescending(x => x.Date)
            .FirstOrDefault();

        if (weather == null)
        {
            return NotFound();
        }

        return Ok(new WeatherViewModel
        {
            Date = weather.Date,
            Temperature = weather.Temperature,
            WindSpeed = weather.WindSpeed,
            WindDirection = weather.WindDirection,
            IconClass = weather.IconClass
        });
    }
}