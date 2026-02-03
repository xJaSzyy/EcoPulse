using EcoPulseBackend.Contexts;
using EcoPulseBackend.Enums;
using EcoPulseBackend.Interfaces;
using EcoPulseBackend.Models;
using EcoPulseBackend.Models.GasolineGenerator;
using EcoPulseBackend.Models.Result;

namespace EcoPulseBackend.Services;

public class GasolineGeneratorService : IGasolineGeneratorService
{
    private readonly ApplicationDbContext _dbContext;

    public GasolineGeneratorService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<EmissionsResult> CalculateEmissionsBatch(GasolineGeneratorEmissionsCalculateModel model)
    {
        var pollutants = new List<Pollutant> { Pollutant.CO, Pollutant.CH, Pollutant.NO2, Pollutant.NO, Pollutant.SO2 };
        
        var result = new List<EmissionsResult>();

        foreach (var pollutant in pollutants.OrderBy(p => (int)p))
        {
            result.Add(CalculateGasolineGeneratorEmissions(pollutant, model));
        }

        return result;
    }
    
    private EmissionsResult CalculateGasolineGeneratorEmissions(Pollutant pollutant, GasolineGeneratorEmissionsCalculateModel model)
    {
        var pollutantInfo = _dbContext.PollutantInfos.First(i => i.Pollutant == pollutant);

        if (!pollutantInfo.SpecificEmission.HasValue)
        {
            return new EmissionsResult();
        }
        
        var maximumEmission = 0.25f * (float)pollutantInfo.SpecificEmission * 5f * model.SameGeneratorCount / 3600f;
        var grossEmission = 0.25f * (float)pollutantInfo.SpecificEmission * 5f * model.WorkHoursPerDay * model.WorkDaysPerYear *
                            model.GeneratorCount * 1e-6f;

        var result = new EmissionsResult
        {
            PollutantInfo = pollutantInfo,
            MaximumEmission = maximumEmission,
            GrossEmission = grossEmission
        };

        return result;
    }
}