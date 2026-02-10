using EcoPulseBackend.Contexts;
using EcoPulseBackend.Models.Enterprise;
using Microsoft.AspNetCore.Mvc;

namespace EcoPulseBackend.Controllers;

[ApiController]
public class EnterpriseController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public EnterpriseController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("enterprise/{id:int}")]
    public IActionResult GetEnterpriseById(int id)
    {
        var enterprise = _dbContext.Enterprises.FirstOrDefault(c => c.Id == id);

        if (enterprise == null)
        {
            return NotFound();
        }

        return Ok(enterprise);
    }

    [HttpPost("enterprise")]
    public async Task<IActionResult> CreateEnterprise([FromBody] EnterpriseAddModel model)
    {
        var enterprise = new Enterprise
        {
            Name = model.Name,
            SanitaryArea = model.SanitaryArea
        };
        
        _dbContext.Enterprises.Add(enterprise);
        await _dbContext.SaveChangesAsync();
        
        return Ok(enterprise);
    }
    
    [HttpPut("enterprise")]
    public async Task<IActionResult> UpdateEnterprise([FromBody] EnterpriseUpdateModel model)
    {
        var enterprise = _dbContext.Enterprises.FirstOrDefault(c => c.Id == model.Id);

        if (enterprise == null)
        {
            return NotFound();
        }
        
        enterprise.Name = model.Name ?? enterprise.Name;
        enterprise.SanitaryArea = model.SanitaryArea ?? enterprise.SanitaryArea;
        
        _dbContext.Enterprises.Update(enterprise);
        await _dbContext.SaveChangesAsync();
        
        return Ok(enterprise);
    }
    
    [HttpPost("enterprise/sanitary-area")]
    public IActionResult GetAllEnterpriseSanitaryAreas([FromBody] List<int> cityIds)
    {
        var sanitaryAreas = _dbContext.Enterprises
            .Where(e => cityIds.Contains(e.CityId))
            .Select(e => e.SanitaryArea)
            .ToList();
        
        return Ok(sanitaryAreas);
    }
}