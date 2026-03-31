using EcoPulseBackend.Contexts;
using EcoPulseBackend.Enums;
using EcoPulseBackend.Interfaces;
using EcoPulseBackend.Models;
using EcoPulseBackend.Models.DangerZone;
using EcoPulseBackend.Models.Result;
using EcoPulseBackend.Models.TrafficLightQueue;
using EcoPulseBackend.Models.TrafficLightQueueEmissionSource;
using EcoPulseBackend.Models.VehicleFlow;
using EcoPulseBackend.Services;
using Moq;
using NetTopologySuite.Geometries;

namespace EcoPulseBackendTests;

public class VehicleQueueServiceTests
{
    private ITrafficLightQueueService _service;
    private Mock<ApplicationDbContext> _dbContextMock;

    [SetUp]
    public void Setup()
    {
        _dbContextMock = new Mock<ApplicationDbContext>();

        _service = new TrafficLightQueueService(_dbContextMock.Object);
    }
    
    [Test]
    public void CalculateEmissionsBatch_WhenValidInput_ShouldReturnCorrectListOfEmissionsResult()
    {
        // Arrange
        var calculateModel = new TrafficLightQueueEmissionsCalculateModel
        {
            VehicleGroups =
            [
                new VehicleGroupQueue
                {
                    TrafficLightQueueEmissionSourceId = 1,
                    VehicleType = VehicleType.DieselBuses,
                    VehiclesCount = 5
                }
            ],
            TrafficLightCycles = 10,
            TrafficLightStopTime = 120
        };

        var emissions = new List<EmissionsResult>
        {
            new()
            {
                PollutantInfo = _pollutantInfos.First(x => x.Pollutant == Pollutant.LeadCompounds),
                MaximumEmission = 0f
            },
            new()
            {
                PollutantInfo = _pollutantInfos.First(x => x.Pollutant == Pollutant.NO2),
                MaximumEmission = 105f
            },
            new()
            {
                PollutantInfo = _pollutantInfos.First(x => x.Pollutant == Pollutant.Soot),
                MaximumEmission = 13.500001f
            },
            new()
            {
                PollutantInfo = _pollutantInfos.First(x => x.Pollutant == Pollutant.SO2),
                MaximumEmission = 13.500001f
            },
            new()
            {
                PollutantInfo = _pollutantInfos.First(x => x.Pollutant == Pollutant.CO),
                MaximumEmission = 460.49997f
            },
            new()
            {
                PollutantInfo = _pollutantInfos.First(x => x.Pollutant == Pollutant.C20H12),
                MaximumEmission = 0.00095999998f
            },
            new()
            {
                PollutantInfo = _pollutantInfos.First(x => x.Pollutant == Pollutant.CH2O),
                MaximumEmission = 2.9999998f
            },
            new()
            {
                PollutantInfo = _pollutantInfos.First(x => x.Pollutant == Pollutant.CH),
                MaximumEmission = 61.5f
            }
        };

        _dbContextMock
            .Setup(x => x.PollutantInfos)
            .Returns(Tests.GetMockDbSet(_pollutantInfos.AsQueryable()).Object);

        // Act
        var result = _service.CalculateEmissionsBatch(calculateModel);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(8));

            for (var emissionIndex = 0; emissionIndex < result.Count; emissionIndex++)
            {
                Assert.That(result[emissionIndex].PollutantInfo.Id,
                    Is.EqualTo(emissions[emissionIndex].PollutantInfo.Id));
                Assert.That(result[emissionIndex].MaximumEmission,
                    Is.EqualTo(emissions[emissionIndex].MaximumEmission));
                Assert.That(result[emissionIndex].GrossEmission, Is.EqualTo(emissions[emissionIndex].GrossEmission));
                Assert.That(result[emissionIndex].Distance, Is.EqualTo(emissions[emissionIndex].Distance));
            }
        });
    }
    
    [Test]
    public async Task CalculateDangerZones_WhenValidInput_ShouldReturnCorrectListOfVehicleFlowDangerZone()
    {
        // Arrange
        var emissionSources = new List<TrafficLightQueueEmissionSource>
        {
            new()
            {
                Id = 1,
                CityId = 1,
                Location = new Point(86.177997, 55.312554),
                VehicleGroups =
                [
                    new VehicleGroupQueue
                    {
                        TrafficLightQueueEmissionSourceId = 1,
                        VehicleType = VehicleType.DieselBuses,
                        VehiclesCount = 5
                    }
                ],
                TrafficLightCycles = 10,
                TrafficLightStopTime = 120
            }
        };

        var expectedResult = new List<TrafficLightQueueDangerZone>
        {
            new()
            {
                EmissionSourceId = emissionSources[0].Id,
                Location = emissionSources[0].Location,
                Color = "rgba(246, 104, 106, 1)",
                AverageConcentration = 105.0f,
                PollutionLevel = "высокий"
            }
        };

        _dbContextMock
            .Setup(x => x.PollutantInfos)
            .Returns(Tests.GetMockDbSet(_pollutantInfos.AsQueryable()).Object);

        // Act
        var result = await _service.CalculateDangerZones(emissionSources);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));

            for (var dangerZoneIndex = 0; dangerZoneIndex < result.Count; dangerZoneIndex++)
            {
                Assert.That(result[dangerZoneIndex].EmissionSourceId, Is.EqualTo(expectedResult[dangerZoneIndex].EmissionSourceId));
                Assert.That(result[dangerZoneIndex].Location, Is.EqualTo(expectedResult[dangerZoneIndex].Location));
                Assert.That(result[dangerZoneIndex].Color, Is.EqualTo(expectedResult[dangerZoneIndex].Color));
                Assert.That(result[dangerZoneIndex].AverageConcentration, Is.EqualTo(expectedResult[dangerZoneIndex].AverageConcentration));
                Assert.That(result[dangerZoneIndex].PollutionLevel, Is.EqualTo(expectedResult[dangerZoneIndex].PollutionLevel));
            }
        });
    }
    
    #region TestData

    private readonly List<PollutantInfo> _pollutantInfos =

    [
        new()
        {
            Id = 2,
            Code = 337,
            Name = "Углерода оксид (углерод окись; углерод моноокись; угарный газ)",
            ShortName = "CO",
            Pollutant = Pollutant.CO,
            SpecificEmission = 7.5f,
            DailyAverageConcentration = 3f,
            MaxPermissibleConcentration = 5f
        },
        new()
        {
            Id = 3,
            Code = 2704,
            Name = "Бензин (нефтяной, малосернистый) /в пересчете на углерод/",
            ShortName = "CH",
            Pollutant = Pollutant.CH,
            SpecificEmission = 1f,
            DailyAverageConcentration = 1.5f,
            MaxPermissibleConcentration = 5f
        },
        new()
        {
            Id = 4,
            Code = 301,
            Name = "Азота диоксид (двуокись азота; пероксид азота)",
            ShortName = "NO2",
            Pollutant = Pollutant.NO2,
            SpecificEmission = 0.112f,
            Mass = 0.2695f,
            MaxPermissibleConcentration = 0.2f,
            DailyAverageConcentration = 0.04f
        },
        new()
        {
            Id = 6,
            Code = 330,
            Name = "Серы диоксид",
            ShortName = "SO2",
            Pollutant = Pollutant.SO2,
            SpecificEmission = 0.036f,
            Mass = 1.0528f,
            MaxPermissibleConcentration = 0.5f,
            DailyAverageConcentration = 0.05f
        },
        new()
        {
            Id = 14,
            Code = 328,
            Name = "Сажа",
            ShortName = "Soot",
            Pollutant = Pollutant.Soot
        },
        new()
        {
            Id = 15,
            Code = 184,
            Name = "Соединения свинца",
            ShortName = "LeadCompounds",
            Pollutant = Pollutant.LeadCompounds,
            MaxPermissibleConcentration = 0.001f,
            DailyAverageConcentration = 0.0003f
        },
        new()
        {
            Id = 16,
            Code = 1325,
            Name = "Формальдегид",
            ShortName = "CH2O",
            Pollutant = Pollutant.CH2O,
            MaxPermissibleConcentration = 0.035f,
            DailyAverageConcentration = 0.003f
        },
        new()
        {
            Id = 17,
            Code = 703,
            Name = "Бенз(а)пирен",
            ShortName = "C20H12",
            Pollutant = Pollutant.C20H12,
        }
    ];

    #endregion
}