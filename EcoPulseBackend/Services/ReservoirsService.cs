using EcoPulseBackend.Contexts;
using EcoPulseBackend.Enums;
using EcoPulseBackend.Interfaces;
using EcoPulseBackend.Models;
using EcoPulseBackend.Models.Reservoirs;
using EcoPulseBackend.Models.Result;

namespace EcoPulseBackend.Services;

public class ReservoirsService : IReservoirsService
{
    private readonly ApplicationDbContext _dbContext;

    public ReservoirsService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public ReservoirsEmissionsBatchResult CalculateEmissionsBatch(ReservoirsEmissionsCalculateModel model)
    {
        var pollutants = new List<Pollutant> { Pollutant.RPK240280, Pollutant.H2S };

        var vaporConcentration = _dbContext.VaporConcentrations.FirstOrDefault(v => v.ReservoirType == model.ReservoirType && v.ClimateZone == model.ClimateZone && v.OilProduct == model.OilProduct);

        if (vaporConcentration == null)
        {
            return new ReservoirsEmissionsBatchResult();
        }
        
        var result = new ReservoirsEmissionsBatchResult
        {
            AnnualInjectionEmissions = (vaporConcentration.AutumnWinterVaporConcentration * model.AutumnWinterOilAmount + vaporConcentration.SpringSummerVaporConcentration * model.SpringSummerOilAmount) * 1e-6f,
            AnnualIrrigationEmissions = 50f * (model.AutumnWinterOilAmount + model.SpringSummerOilAmount) * 1e-6f,
            MaxVaporEmission = (vaporConcentration.MaxVaporConcentration * model.DrainedVolume) / model.AverageDrainTime,
            Emissions = new List<EmissionsResult>()
        };
        
        foreach (var pollutant in pollutants.OrderBy(p => (int)p))
        {
            result.Emissions.Add(CalculateReservoirsEmissions(pollutant, vaporConcentration, model));
        }
        
        return result;
    }
    
    private EmissionsResult CalculateReservoirsEmissions(Pollutant pollutant, VaporConcentration vaporConcentration, ReservoirsEmissionsCalculateModel model)
    {
        var pollutantInfo = _dbContext.PollutantInfos.First(i => i.Pollutant == pollutant);

        if (!pollutantInfo.SpecificEmission.HasValue)
        {
            return new EmissionsResult();
        }
        
        var annualInjectionEmissions = (vaporConcentration.AutumnWinterVaporConcentration * model.AutumnWinterOilAmount +
                                        vaporConcentration.SpringSummerVaporConcentration * model.SpringSummerOilAmount) *
                                       1e-6f;
        var annualIrrigationEmissions = 50f * (model.AutumnWinterOilAmount + model.SpringSummerOilAmount) * 1e-6f;

        var maxVaporEmission = (vaporConcentration.MaxVaporConcentration * model.DrainedVolume) / model.AverageDrainTime;
        var grossEmission = annualInjectionEmissions + annualIrrigationEmissions;

        var result = new EmissionsResult
        {
            PollutantInfo = pollutantInfo,
            MaximumEmission = maxVaporEmission * (float)pollutantInfo.SpecificEmission * 1e-2f,
            GrossEmission = grossEmission * (float)pollutantInfo.SpecificEmission * 1e-2f,
        };

        return result;
    }
}