using EcoPulseBackend.Contexts;
using EcoPulseBackend.Interfaces;
using EcoPulseBackend.Models.MaximumSingle;
using Microsoft.AspNetCore.Mvc;

namespace EcoPulseBackend.Controllers;

[ApiController]
public class EmissionController : ControllerBase
{
    private readonly ILogger<EmissionController> _logger;
    private readonly IEmissionService _service;
    private readonly ApplicationDbContext _dbContext;

    public EmissionController(ILogger<EmissionController> logger, 
        IEmissionService service, 
        ApplicationDbContext dbContext)
    {
        _logger = logger;
        _service = service;
        _dbContext = dbContext;
    }
    
    [HttpPost("emission/maximum-single")]
    public IActionResult CalculateMaximumSingleEmissions([FromBody] MaximumSingleEmissionsCalculateModel model)
    {
        var result = _service.MaximumSingleService.CalculateEmissions(model);

        return Ok(result);
    }
}