using EcoPulseBackend.Contexts;
using EcoPulseBackend.Enums;
using EcoPulseBackend.Interfaces;
using EcoPulseBackend.Models;
using EcoPulseBackend.Models.DuringWeldingOperations;
using EcoPulseBackend.Models.Result;

namespace EcoPulseBackend.Services;

public class DuringWeldingOperationsService : IDuringWeldingOperationsService
{
    private readonly ApplicationDbContext _dbContext;

    public DuringWeldingOperationsService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public DuringWeldingOperationsEmissionsBatchResult CalculateEmissionsBatch(DuringWeldingOperationsEmissionsCalculateModel model)
    {
        var pollutants = new List<Pollutant> { Pollutant.Fe2O3, Pollutant.MnO2, Pollutant.FluorideGases };
        
        var normElectrodesPerYear = model.ElectrodesPerYear * (100f - 15f) * 1e-2f;
        var materialsConsumption = normElectrodesPerYear / model.WorkDaysPerYear;
        
        var result = new DuringWeldingOperationsEmissionsBatchResult
        {
            NormElectrodesPerYear = normElectrodesPerYear,
            MaterialsConsumption = materialsConsumption,
            Emissions = []
        };

        foreach (var pollutant in pollutants.OrderBy(p => (int)p))
        {
            result.Emissions.Add(CalculateDuringWeldingOperationsEmissions(pollutant, model.WorkDaysPerYear, materialsConsumption));
        }

        return result;
    }
    
    private EmissionsResult CalculateDuringWeldingOperationsEmissions(Pollutant pollutant, int workDaysPerYear, float materialsConsumption)
    {
        var pollutantInfo = _dbContext.PollutantInfos.First(i => i.Pollutant == pollutant);
        pollutantInfo.SpecificEmission = SpecificEmissionsByElectrodes.GetValueOrDefault(pollutant, 0f);
        
        if (!pollutantInfo.SpecificEmission.HasValue)
        {
            return new EmissionsResult();
        }
        
        var maximumEmission = materialsConsumption * (float)pollutantInfo.SpecificEmission * 0.4f / 3600;
        var grossEmission = maximumEmission * workDaysPerYear * 3.6f * 1e-3f;
        
        var result = new EmissionsResult
        {
            PollutantInfo = pollutantInfo,
            MaximumEmission = maximumEmission,
            GrossEmission = grossEmission
        };
        
        return result;
    }
    
    private static readonly Dictionary<Pollutant, float> SpecificEmissionsByElectrodes = new()
    {
        { Pollutant.Fe2O3, 9.77f },
        { Pollutant.MnO2, 1.73f },
        { Pollutant.FluorideGases, 0.40f }
    };
}