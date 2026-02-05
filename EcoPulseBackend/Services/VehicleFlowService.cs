using EcoPulseBackend.Contexts;
using EcoPulseBackend.Enums;
using EcoPulseBackend.Extensions;
using EcoPulseBackend.Interfaces;
using EcoPulseBackend.Models.DangerZone;
using EcoPulseBackend.Models.Result;
using EcoPulseBackend.Models.VehicleFlow;
using EcoPulseBackend.Models.VehicleFlowEmissionSource;

namespace EcoPulseBackend.Services;

public class VehicleFlowService : IVehicleFlowService
{
    private readonly ApplicationDbContext _dbContext;

    public VehicleFlowService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<EmissionsResult> CalculateEmissionsBatch(VehicleFlowEmissionsCalculateModel model)
    {
        var pollutants = new List<Pollutant>
        {
            Pollutant.CO, Pollutant.NO2, Pollutant.CH, Pollutant.Soot,
            Pollutant.SO2, Pollutant.LeadCompounds, Pollutant.CH2O, Pollutant.C20H12
        };

        return pollutants.OrderBy(p => (int)p).Select(pollutant => CalculateVehicleFlowEmissions(pollutant, model)).OfType<EmissionsResult>().ToList();
    }
    
    public List<VehicleFlowDangerZone> CalculateDangerZones(List<VehicleFlowEmissionSource> emissionSources)
    {
        var result = new List<VehicleFlowDangerZone>();
        
        foreach (var source in emissionSources)
        {
            var points = source.Points;
            
            float length = 0;
            for (var i = 1; i < points.Count; i++)
            {
                length += (float)points[i - 1].Distance(points[i]);
            }
            
            var calculateModel = new VehicleFlowEmissionsCalculateModel
            {
                VehicleGroups =
                [
                    new VehicleGroup
                    {
                        VehicleType = source.VehicleType,
                        MaxTrafficIntensity = source.MaxTrafficIntensity * (length / 1000f),
                        AverageSpeed = source.AverageSpeed
                    }
                ],
                Length = length
            };
            
            var emissionsResult = CalculateVehicleFlowEmissions(Pollutant.NO2, calculateModel);

            if (emissionsResult == null)
            {
                return [];
            }
            
            var maximumEmission = emissionsResult.MaximumEmission;
            var color = DangerZoneUtils.GetColorByConcentration(maximumEmission);
            
            result.Add(new VehicleFlowDangerZone
            {
                EmissionSourceId = source.Id,
                Points = points,
                Color = color,
                AverageConcentration = emissionsResult.MaximumEmission
            });
        }

        return result;
    }
    
    private EmissionsResult? CalculateVehicleFlowEmissions(Pollutant pollutant, VehicleFlowEmissionsCalculateModel model)
    {
        var pollutantInfo = _dbContext.PollutantInfos.First(i => i.Pollutant == pollutant);
        
        if (!VehicleEmissionFactors[model.VehicleGroups.First().VehicleType].ContainsKey(pollutant))
        {
            return null;
        }
        
        var emission = 0f;

        foreach (var vehicleGroup in model.VehicleGroups)
        {
            var specificEmission = VehicleEmissionFactors[vehicleGroup.VehicleType][pollutant];

            var speedCorrectionFactor = GetSpeedCorrectionFactor(vehicleGroup.AverageSpeed);
            
            emission += specificEmission * vehicleGroup.MaxTrafficIntensity * speedCorrectionFactor;
        }
        
        emission *= model.Length / 3600f;

        var result = new EmissionsResult
        {
            MaximumEmission = emission,
            PollutantInfo =  pollutantInfo
        };
        
        return result;
    }

    private static float GetSpeedCorrectionFactor(double speed)
    {
        var nearest = SpeedCorrectionFactors
            .Where(x => x.Key >= speed)
            .OrderBy(x => x.Key)
            .FirstOrDefault();
    
        return nearest.Key == 0 ? SpeedCorrectionFactors.Values.Last() : nearest.Value;
    }
    
    private static readonly Dictionary<int, float> SpeedCorrectionFactors = new()
    {
        { 10, 1.35f },
        { 15, 1.28f },
        { 20, 1.2f },
        { 25, 1.1f },
        { 30, 1f },
        { 35, 0.88f },
        { 40, 0.75f },
        { 45, 0.63f },
        { 50, 0.5f },
        { 60, 0.3f },
        { 75, 0.45f },
        { 80, 0.5f },
        { 100, 0.65f }
    };
    
    private static readonly Dictionary<VehicleType, Dictionary<Pollutant, float>> VehicleEmissionFactors = new()
    {
        {
            VehicleType.Passenger, new Dictionary<Pollutant, float>
            {
                { Pollutant.CO, 19f },
                { Pollutant.NO2, 1.8f },
                { Pollutant.CH, 2.1f },
                { Pollutant.SO2, 0.065f },
                { Pollutant.CH2O, 0.006f },
                { Pollutant.LeadCompounds, 0.019f },
                { Pollutant.C20H12, 1.7f * 1e-6f },
            }
        },
        {
            VehicleType.DieselPassenger, new Dictionary<Pollutant, float>
            {
                { Pollutant.CO, 2f },
                { Pollutant.NO2, 1.3f },
                { Pollutant.CH, 0.25f },
                { Pollutant.Soot, 0.1f },
                { Pollutant.SO2, 0.21f },
                { Pollutant.CH2O, 0.003f },
            }
        },
        {
            VehicleType.CargoCarburetorLow, new Dictionary<Pollutant, float>()
            {
                { Pollutant.CO, 69.4f },
                { Pollutant.NO2, 2.9f },
                { Pollutant.CH, 11.5f },
                { Pollutant.SO2, 0.2f },
                { Pollutant.CH2O, 0.02f },
                { Pollutant.LeadCompounds, 0.026f },
                { Pollutant.C20H12, 4.5f * 1e-6f },
            }
        },
        {
            VehicleType.CargoCarburetorHigh, new Dictionary<Pollutant, float>()
            {
                { Pollutant.CO, 75f },
                { Pollutant.NO2, 5.2f },
                { Pollutant.CH, 13.4f },
                { Pollutant.SO2, 0.22f },
                { Pollutant.CH2O, 0.022f },
                { Pollutant.LeadCompounds, 0.033f },
                { Pollutant.C20H12, 6.3f * 1e-6f },
            }
        },
        {
            VehicleType.CarburetorBuses, new Dictionary<Pollutant, float>()
            {
                { Pollutant.CO, 97.6f },
                { Pollutant.NO2, 5.3f },
                { Pollutant.CH, 13.4f },
                { Pollutant.SO2, 0.32f },
                { Pollutant.CH2O, 0.03f },
                { Pollutant.LeadCompounds, 0.041f },
                { Pollutant.C20H12, 6.4f * 1e-6f },
            }
        },
        {
            VehicleType.DieselTrucks, new Dictionary<Pollutant, float>()
            {
                { Pollutant.CO, 8.5f },
                { Pollutant.NO2, 7.7f },
                { Pollutant.CH, 6f },
                { Pollutant.Soot, 0.3f },
                { Pollutant.SO2, 1.25f },
                { Pollutant.CH2O, 0.21f },
                { Pollutant.C20H12, 6.5f * 1e-6f },
            }
        },
        {
            VehicleType.DieselBuses, new Dictionary<Pollutant, float>()
            {
                { Pollutant.CO, 8.8f },
                { Pollutant.NO2, 8f },
                { Pollutant.CH, 6.5f },
                { Pollutant.Soot, 0.3f },
                { Pollutant.SO2, 1.45f },
                { Pollutant.CH2O, 0.31f },
                { Pollutant.C20H12, 6.7f * 1e-6f },
            }
        },
        {
            VehicleType.CargoGas, new Dictionary<Pollutant, float>()
            {
                { Pollutant.CO, 39f },
                { Pollutant.NO2, 2.6f },
                { Pollutant.CH, 1.3f },
                { Pollutant.SO2, 0.18f },
                { Pollutant.CH2O, 0.002f },
                { Pollutant.C20H12, 2f * 1e-6f },
            }
        }
    };
}