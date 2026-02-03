using EcoPulseBackend.Contexts;
using EcoPulseBackend.Interfaces;
using EcoPulseBackend.Models.DuringMetalMachining;
using EcoPulseBackend.Models.DuringWeldingOperations;
using EcoPulseBackend.Models.GasolineGenerator;
using EcoPulseBackend.Models.MaximumSingle;
using EcoPulseBackend.Models.OpenCoalWarehouse;
using EcoPulseBackend.Models.Reservoirs;
using EcoPulseBackend.Models.TrafficLightQueue;
using EcoPulseBackend.Models.VehicleFlow;
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
    
    [HttpPost("emission/gasoline-generator")]
    public IActionResult CalculateGasolineGenerator([FromBody] GasolineGeneratorEmissionsCalculateModel model)
    {
        var result = _service.GasolineGeneratorService.CalculateEmissionsBatch(model);

        return Ok(result);
    }

    [HttpPost("emission/reservoirs")]
    public IActionResult CalculateReservoirs([FromBody] ReservoirsEmissionsCalculateModel model)
    {
        var result = _service.ReservoirsService.CalculateEmissionsBatch(model);

        return Ok(result.Emissions);
    }
    
    [HttpPost("emission/during-metal-machining")]
    public IActionResult CalculateDuringMetalMachining([FromBody] DuringMetalMachiningEmissionsCalculateModel model)
    { 
        var result = _service.DuringMetalMachiningService.CalculateEmissionsBatch(model);

        return Ok(result);
    }
    
    [HttpPost("emission/during-welding-operations")]
    public IActionResult CalculateDuringWeldingOperations([FromBody] DuringWeldingOperationsEmissionsCalculateModel model)
    {
        var result = _service.DuringWeldingOperationsService.CalculateEmissionsBatch(model);

        return Ok(result.Emissions);
    }
    
    [HttpPost("emission/maximum-single")]
    public IActionResult CalculateMaximumSingleEmissions([FromBody] MaximumSingleEmissionsCalculateModel model)
    {
        var result = _service.MaximumSingleService.CalculateEmissions(model);

        return Ok(result);
    }
    
    [HttpPost("emission/vehicle-flow")]
    public IActionResult CalculateVehicleFlowEmissions([FromBody] VehicleFlowEmissionsCalculateModel model)
    {
        var result = _service.VehicleFlowService.CalculateEmissionsBatch(model);

        return Ok(result);
    }
    
    [HttpPost("emission/traffic-light-queue")]
    public IActionResult CalculateTrafficLightQueueEmissions([FromBody] TrafficLightQueueEmissionsCalculateModel model)
    {
        var result = _service.TrafficLightQueueService.CalculateEmissionsBatch(model);

        return Ok(result);
    }

    [HttpPost("emission/open-coal-warehouse")]
    public IActionResult CalculateOpenCoalWarehouseEmissions([FromBody] OpenCoalWarehouseEmissionsCalculateModel model)
    {
        var result = _service.OpenCoalWarehouseService.CalculateEmissions(model);

        return Ok(result);
    }
}