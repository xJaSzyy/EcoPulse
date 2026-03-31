using EcoPulseBackend.Contexts;
using EcoPulseBackend.Enums;
using EcoPulseBackend.Interfaces;
using EcoPulseBackend.Models;
using EcoPulseBackend.Models.MaximumSingle;
using EcoPulseBackend.Models.Result;
using EcoPulseBackend.Models.VehicleFlow;
using EcoPulseBackend.Models.VehicleFlowEmissionSource;
using EcoPulseBackend.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite.Geometries;

namespace EcoPulseBackendTests;

public class VehicleFlowServiceTests
{
    private IVehicleFlowService _service;
    private Mock<ApplicationDbContext> _dbContextMock;
    
    [SetUp]
    public void Setup()
    {
        _dbContextMock = new Mock<ApplicationDbContext>();
        
        _service = new VehicleFlowService(_dbContextMock.Object);
    }

    [Test]
    public void CalculateEmissionsBatch_WhenValidInput_ShouldReturnCorrectListOfEmissionsResult()
    {
        // Arrange
        var calculateModel = new VehicleFlowEmissionsCalculateModel
        {
            VehicleGroups =
            [
                new VehicleGroup
                {
                    AverageSpeed = 90,
                    MaxTrafficIntensity = 8,
                    VehicleType = VehicleType.DieselBuses
                }
            ],
            Length = 1000
        };

        var emissions = new List<EmissionsResult>
        {
            new()
            {
                PollutantInfo = _pollutantInfos.First(x => x.Pollutant == Pollutant.NO2),
                MaximumEmission = 11.555555f
            },
            new()
            {
                PollutantInfo = _pollutantInfos.First(x => x.Pollutant == Pollutant.Soot),
                MaximumEmission = 0.43333336f
            },
            new()
            {
                PollutantInfo = _pollutantInfos.First(x => x.Pollutant == Pollutant.SO2),
                MaximumEmission = 2.0944445f
            },
            new()
            {
                PollutantInfo = _pollutantInfos.First(x => x.Pollutant == Pollutant.CO),
                MaximumEmission = 12.711111f
            },
            new()
            {
                PollutantInfo = _pollutantInfos.First(x => x.Pollutant == Pollutant.C20H12),
                MaximumEmission = 0.0000096777769f
            },
            new()
            {
                PollutantInfo = _pollutantInfos.First(x => x.Pollutant == Pollutant.CH2O),
                MaximumEmission = 0.4477778f
            },
            new()
            {
                PollutantInfo = _pollutantInfos.First(x => x.Pollutant == Pollutant.CH),
                MaximumEmission = 9.388889f
            }
        };

        _dbContextMock
            .Setup(x => x.PollutantInfos)
            .Returns(GetMockDbSet(_pollutantInfos.AsQueryable()).Object);
        
        // Act
        var result = _service.CalculateEmissionsBatch(calculateModel);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(7));

            for (var emissionIndex = 0; emissionIndex < result.Count; emissionIndex++)
            {
                Assert.That(result[emissionIndex].PollutantInfo.Id, Is.EqualTo(emissions[emissionIndex].PollutantInfo.Id));
                Assert.That(result[emissionIndex].MaximumEmission, Is.EqualTo(emissions[emissionIndex].MaximumEmission));
                Assert.That(result[emissionIndex].GrossEmission, Is.EqualTo(emissions[emissionIndex].GrossEmission));
                Assert.That(result[emissionIndex].Distance, Is.EqualTo(emissions[emissionIndex].Distance));
            }
        });
    }
    
    [Test]
    public async Task CalculateDangerZones_WhenValidInput_ShouldReturnCorrectListOfVehicleFlowDangerZone()
    {
        // Arrange
        var emissionSources = new List<VehicleFlowEmissionSource>
        {
            new()
            {
                Id = 1,
                CityId = 1,
                StreetName = "",
                Points = new LineString([new Coordinate(86.177997, 55.312554), new Coordinate(86.147901, 55.345084)]),
                VehicleType = VehicleType.DieselBuses,
                MaxTrafficIntensity = 8,
                AverageSpeed = 90,
                UpdatedAt = DateTime.UtcNow,
            }
        };

        _dbContextMock
            .Setup(x => x.PollutantInfos)
            .Returns(GetMockDbSet(_pollutantInfos.AsQueryable()).Object);
        
        // Act
        var result = await _service.CalculateDangerZones(emissionSources);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));

            for (var dangerZoneIndex = 0; dangerZoneIndex < result.Count; dangerZoneIndex++)
            {
                Assert.That(result[dangerZoneIndex].EmissionSourceId, Is.EqualTo(1));
                Assert.That(result[dangerZoneIndex].Points, Is.EqualTo(emissionSources[dangerZoneIndex].Points));
                Assert.That(result[dangerZoneIndex].Color, Is.EqualTo("rgba(251, 153, 86, 1)"));
                Assert.That(result[dangerZoneIndex].AverageConcentration, Is.EqualTo(47.23394f));
                Assert.That(result[dangerZoneIndex].PollutionLevel, Is.EqualTo("средний"));
            }
        });
    }
    
    private static Mock<DbSet<T>> GetMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator);

        return mockSet;
    }

    #region TestData

    private readonly List<PollutantInfo> _pollutantInfos =
    [
        new PollutantInfo
        {
            Id = 2, Code = 337, Name = "Углерода оксид (углерод окись; углерод моноокись; угарный газ)",
            ShortName = "CO", Pollutant = Pollutant.CO,
            SpecificEmission = 7.5f, DailyAverageConcentration = 3f, MaxPermissibleConcentration = 5f
        },
        new PollutantInfo
        {
            Id = 3, Code = 2704, Name = "Бензин (нефтяной, малосернистый) /в пересчете на углерод/",
            ShortName = "CH", Pollutant = Pollutant.CH,
            SpecificEmission = 1f, DailyAverageConcentration = 1.5f, MaxPermissibleConcentration = 5f
        },
        new PollutantInfo
        {
            Id = 4, Code = 301, Name = "Азота диоксид (двуокись азота; пероксид азота)", ShortName = "NO2",
            Pollutant = Pollutant.NO2,
            SpecificEmission = 0.112f, Mass = 0.2695f, MaxPermissibleConcentration = 0.2f,
            DailyAverageConcentration = 0.04f
        },
        new PollutantInfo
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
        new PollutantInfo
        {
            Id = 14,
            Code = 328,
            Name = "Сажа",
            ShortName = "Soot",
            Pollutant = Pollutant.Soot
        },
        new PollutantInfo
        {
            Id = 15,
            Code = 184,
            Name = "Соединения свинца",
            ShortName = "LeadCompounds",
            Pollutant = Pollutant.LeadCompounds,
            MaxPermissibleConcentration = 0.001f,
            DailyAverageConcentration = 0.0003f
        },
        new PollutantInfo
        {
            Id = 16,
            Code = 1325,
            Name = "Формальдегид",
            ShortName = "CH2O",
            Pollutant = Pollutant.CH2O,
            MaxPermissibleConcentration = 0.035f,
            DailyAverageConcentration = 0.003f
        },
        new PollutantInfo
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