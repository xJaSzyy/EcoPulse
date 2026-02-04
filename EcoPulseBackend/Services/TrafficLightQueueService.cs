using EcoPulseBackend.Contexts;
using EcoPulseBackend.Enums;
using EcoPulseBackend.Extensions;
using EcoPulseBackend.Interfaces;
using EcoPulseBackend.Models.DangerZone;
using EcoPulseBackend.Models.Result;
using EcoPulseBackend.Models.TrafficLightQueue;
using EcoPulseBackend.Models.TrafficLightQueueEmissionSource;

namespace EcoPulseBackend.Services;

public class TrafficLightQueueService : ITrafficLightQueueService
{
    private readonly ApplicationDbContext _dbContext;

    public TrafficLightQueueService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<EmissionsResult> CalculateEmissionsBatch(TrafficLightQueueEmissionsCalculateModel model)
    {
        var pollutants = new List<Pollutant>
        {
            Pollutant.CO, Pollutant.NO2, Pollutant.CH, /*Pollutant.Soot,*/
            Pollutant.SO2, Pollutant.LeadCompounds, Pollutant.CH2O, Pollutant.C20H12
        };

        return pollutants.OrderBy(p => (int)p).Select(pollutant => CalculateTrafficLightQueueEmissions(pollutant, model)).OfType<EmissionsResult>().ToList();
    }
    
    public List<TrafficLightQueueDangerZone> CalculateDangerZones(List<TrafficLightQueueEmissionSource> emissionSources)
    {
        var result = new List<TrafficLightQueueDangerZone>();
        
        foreach (var source in emissionSources)
        {
            var calculateModel = new TrafficLightQueueEmissionsCalculateModel
            {
                VehicleGroups = source.VehicleGroups,
                TrafficLightCycles = source.TrafficLightCycles,
                TrafficLightStopTime = source.TrafficLightStopTime
            };
            
            var emissionsResult = CalculateTrafficLightQueueEmissions(Pollutant.NO2, calculateModel);

            if (emissionsResult == null)
            {
                return [];
            }
            
            var maximumEmission = emissionsResult.MaximumEmission;
            var color = DangerZoneUtils.GetColorByConcentration(maximumEmission);
            
            result.Add(new TrafficLightQueueDangerZone
            {
                EmissionSourceId = source.Id,
                Location = source.Location,
                Color = color,
                AverageConcentration = emissionsResult.MaximumEmission
            });
        }

        return result;
    }
    
    private EmissionsResult? CalculateTrafficLightQueueEmissions(Pollutant pollutant, TrafficLightQueueEmissionsCalculateModel model)
    {
        var pollutantInfo = _dbContext.PollutantInfos.First(i => i.Pollutant == pollutant);
        
        var emission = 0f;

        foreach (var vehicleGroup in model.VehicleGroups)
        {
            var specificEmission = VehicleSpecificEmissions[vehicleGroup.VehicleType][pollutant];

            emission += specificEmission * vehicleGroup.VehiclesCount;
        }
        
        emission *= model.TrafficLightCycles * model.TrafficLightStopTime / 40f;

        var result = new EmissionsResult
        {
            MaximumEmission = emission,
            PollutantInfo =  pollutantInfo
        };
        
        return result;
    }

    private static readonly Dictionary<VehicleType, Dictionary<Pollutant, float>> VehicleSpecificEmissions = new()
    {
        {
            VehicleType.Passenger, new Dictionary<Pollutant, float>
            {
                { Pollutant.CO, 3.5f },
                { Pollutant.NO2, 0.05f },
                { Pollutant.CH, 0.25f },
                { Pollutant.SO2, 0.01f },
                { Pollutant.CH2O, 0.0008f },
                { Pollutant.LeadCompounds, 0.0044f },
                { Pollutant.C20H12, 2f * 1e-6f },
            }
        },
        {
            VehicleType.DieselPassenger, new Dictionary<Pollutant, float>
            {
                { Pollutant.CO, 0.13f },
                { Pollutant.NO2, 0.08f },
                { Pollutant.CH, 0.06f },
                { Pollutant.Soot, 0.035f },
                { Pollutant.SO2, 0.04f },
                { Pollutant.CH2O, 0.0008f },
            }
        },
        {
            VehicleType.CargoCarburetorLow, new Dictionary<Pollutant, float>()
            {
                { Pollutant.CO, 6.3f },
                { Pollutant.NO2, 0.075f },
                { Pollutant.CH, 1f },
                { Pollutant.SO2, 0.02f },
                { Pollutant.CH2O, 0.0015f },
                { Pollutant.LeadCompounds, 0.0047f },
                { Pollutant.C20H12, 4f * 1e-6f },
            }
        },
        {
            VehicleType.CargoCarburetorHigh, new Dictionary<Pollutant, float>()
            {
                { Pollutant.CO, 18.4f },
                { Pollutant.NO2, 0.2f },
                { Pollutant.CH, 2.96f },
                { Pollutant.SO2, 0.028f },
                { Pollutant.CH2O, 0.006f },
                { Pollutant.LeadCompounds, 0.0075f },
                { Pollutant.C20H12, 4.5f * 1e-6f },
            }
        },
        {
            VehicleType.CarburetorBuses, new Dictionary<Pollutant, float>()
            {
                { Pollutant.CO, 16.1f },
                { Pollutant.NO2, 0.16f },
                { Pollutant.CH, 2.64f },
                { Pollutant.SO2, 0.03f },
                { Pollutant.CH2O, 0.012f },
                { Pollutant.LeadCompounds, 0.0075f },
                { Pollutant.C20H12, 4.5f * 1e-6f },
            }
        },
        {
            VehicleType.DieselTrucks, new Dictionary<Pollutant, float>()
            {
                { Pollutant.CO, 2.85f },
                { Pollutant.NO2, 0.81f },
                { Pollutant.CH, 0.3f },
                { Pollutant.Soot, 0.07f },
                { Pollutant.SO2, 0.075f },
                { Pollutant.CH2O, 0.015f },
                { Pollutant.C20H12, 6.3f * 1e-6f },
            }
        },
        {
            VehicleType.DieselBuses, new Dictionary<Pollutant, float>()
            {
                { Pollutant.CO, 3.07f },
                { Pollutant.NO2, 0.7f },
                { Pollutant.CH, 0.41f },
                { Pollutant.Soot, 0.09f },
                { Pollutant.SO2, 0.09f },
                { Pollutant.CH2O, 0.02f },
                { Pollutant.C20H12, 6.4f * 1e-6f },
            }
        },
        {
            VehicleType.CargoGas, new Dictionary<Pollutant, float>()
            {
                { Pollutant.CO, 6.44f },
                { Pollutant.NO2, 0.09f },
                { Pollutant.CH, 0.26f },
                { Pollutant.SO2, 0.01f },
                { Pollutant.CH2O, 0.0004f },
                { Pollutant.C20H12, 3.6f * 1e-6f },
            }
        }
    };
}