using EcoPulseBackend.Contexts;
using EcoPulseBackend.Enums;
using EcoPulseBackend.Interfaces;
using EcoPulseBackend.Models.DuringMetalMachining;
using EcoPulseBackend.Models.Result;

namespace EcoPulseBackend.Services;

public class DuringMetalMachiningService : IDuringMetalMachiningService
{
    private readonly ApplicationDbContext _dbContext;

    public DuringMetalMachiningService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<EmissionsResult> CalculateEmissionsBatch(DuringMetalMachiningEmissionsCalculateModel model)
    {
        var pollutants = new List<Pollutant> { Pollutant.Fe2O3 };

        return pollutants.OrderBy(p => (int)p).Select(pollutant => CalculateDuringMetalMachiningEmissions(pollutant, model)).ToList();
    }
    
    private EmissionsResult CalculateDuringMetalMachiningEmissions(Pollutant pollutant, DuringMetalMachiningEmissionsCalculateModel model)
    {
        var pollutantInfo = _dbContext.PollutantInfos.First(i => i.Pollutant == pollutant);
        pollutantInfo.SpecificEmission = SpecificDustEmissionsByType.GetValueOrDefault(model.MetalMachiningMachineType, 0f);

        if (!pollutantInfo.SpecificEmission.HasValue)
        {
            return new EmissionsResult();
        }
        
        var maximumEmission = 0.2f * (float)pollutantInfo.SpecificEmission;
        var grossEmission = 0.2f * 3.6f * (float)pollutantInfo.SpecificEmission * model.WorkDaysPerYear * 1e-3f;

        var result = new EmissionsResult
        {
            PollutantInfo = pollutantInfo,
            MaximumEmission = maximumEmission,
            GrossEmission = grossEmission
        };
        
        return result;
    }

    private static readonly Dictionary<MetalMachiningMachineType, float> SpecificDustEmissionsByType = new()
    {
        { MetalMachiningMachineType.Drilling, 0.007f },
        { MetalMachiningMachineType.Milling, 0.097f },
        { MetalMachiningMachineType.Cutting, 0.203f }
    };
}