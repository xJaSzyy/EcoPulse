using EcoPulseBackend.Contexts;
using EcoPulseBackend.Enums;
using EcoPulseBackend.Interfaces;
using EcoPulseBackend.Models;
using EcoPulseBackend.Models.MaximumSingle;
using EcoPulseBackend.Models.Result;
using EcoPulseBackend.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EcoPulseBackendTests;

public class MaximumSingleServiceTests
{
    private IMaximumSingleService _service;
    private Mock<ApplicationDbContext> _dbContextMock;
    
    [SetUp]
    public void Setup()
    {
        _dbContextMock = new Mock<ApplicationDbContext>();
        
        _service = new MaximumSingleService(_dbContextMock.Object);
    }

    [Test]
    public void CalculateEmissions_WhenValidInput_ShouldReturnCorrectEmissionsGroupResult()
    {
        // Arrange
        var calculateModel = new MaximumSingleEmissionsCalculateModel
        {
            Pollutant = Pollutant.SP,
            EjectedTemp = 235,
            AirTemp = 10,
            AvgExitSpeed = 17,
            HeightSource = 22,
            DiameterSource = 3,
            TempStratificationRatio = CoefficientRegion.CentralRegions,
            SedimentationRateRatio = CoefficientDegreePurification.Medium,
            WindSpeed = 3,
            WindDirection = 90,
            Distance = 1000,
            MaxCount = 1
        };

        var emissions = new List<EmissionsResult>
        {
            new()
            {
                MaximumEmission = 0.265383929f,
                Distance = 475
            }
        };

        var pollutantInfo = new PollutantInfo
        {
            Id = 1, Code = 2, Name = "Твердые частицы", ShortName = "PM2.5", Pollutant = Pollutant.SP,
            Mass = 15.72f, MaxPermissibleConcentration = 0.5f
        };

        _dbContextMock
            .Setup(x => x.PollutantInfos)
            .Returns(GetMockDbSet(new List<PollutantInfo> { pollutantInfo }.AsQueryable()).Object);
        
        // Act
        var result = _service.CalculateEmissions(calculateModel);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.PollutantInfo, Is.EqualTo(pollutantInfo));
            Assert.That(result.Emissions, Has.Count.EqualTo(calculateModel.MaxCount));

            for (var emissionIndex = 0; emissionIndex < result.Emissions.Count; emissionIndex++)
            {
                Assert.That(result.Emissions[emissionIndex].PollutantInfo, Is.EqualTo(emissions[emissionIndex].PollutantInfo));
                Assert.That(result.Emissions[emissionIndex].MaximumEmission, Is.EqualTo(emissions[emissionIndex].MaximumEmission));
                Assert.That(result.Emissions[emissionIndex].GrossEmission, Is.EqualTo(emissions[emissionIndex].GrossEmission));
                Assert.That(result.Emissions[emissionIndex].Distance, Is.EqualTo(emissions[emissionIndex].Distance));
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
}