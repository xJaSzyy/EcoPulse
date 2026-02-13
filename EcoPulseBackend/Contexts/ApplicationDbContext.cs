using EcoPulseBackend.Enums;
using EcoPulseBackend.Models;
using EcoPulseBackend.Models.City;
using EcoPulseBackend.Models.Enterprise;
using EcoPulseBackend.Models.SingleEmissionSource;
using EcoPulseBackend.Models.TrafficLightQueue;
using EcoPulseBackend.Models.TrafficLightQueueEmissionSource;
using EcoPulseBackend.Models.VehicleFlowEmissionSource;
using EcoPulseBackend.Models.Weather;
using Microsoft.EntityFrameworkCore;

namespace EcoPulseBackend.Contexts;

public class ApplicationDbContext : DbContext
{
    public virtual DbSet<PollutantInfo> PollutantInfos { get; set; } = null!;
    public virtual DbSet<VaporConcentration> VaporConcentrations { get; set; } = null!;
    public virtual DbSet<VehicleSpecificEmission> VehicleSpecificEmissions { get; set; } = null!;
    public virtual DbSet<SingleEmissionSource> SingleEmissionSources { get; set; } = null!;
    public virtual DbSet<VehicleFlowEmissionSource> VehicleFlowEmissionSources { get; set; } = null!;
    public virtual DbSet<TrafficLightQueueEmissionSource> TrafficLightQueueEmissionSources { get; set; } = null!;
    public virtual DbSet<VehicleGroupQueue> VehicleGroupQueues { get; set; } = null!;
    public virtual DbSet<City> Cities { get; set; } = null!;
    public virtual DbSet<Enterprise> Enterprises { get; set; } = null!;
    public virtual DbSet<Weather> Weathers { get; set; } = null!;
    
    public ApplicationDbContext() {  }
    
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.HasPostgresExtension("postgis");
    
        builder.Entity<SingleEmissionSource>(entity =>
        {
            entity.Property(e => e.Location)
                .HasColumnType("geometry(Point, 4326)");  
        
            entity.HasIndex(e => e.Location).HasMethod("GIST");
        });
        
        builder.Entity<VehicleFlowEmissionSource>(entity =>
        {
            entity.Property(e => e.Points)
                .HasColumnType("geometry(LineString, 4326)");
    
            entity.HasIndex(e => e.Points).HasMethod("GIST");
        });

        builder.Entity<TrafficLightQueueEmissionSource>(entity =>
        {
            entity.Property(e => e.Location)
                .HasColumnType("geometry(Point, 4326)");  
        
            entity.HasIndex(e => e.Location).HasMethod("GIST");
        });
        
        builder.Entity<City>(entity =>
        {
            entity.Property(e => e.Location)
                .HasColumnType("geometry(Point, 4326)");

            entity.Property(e => e.Polygon)
                .HasColumnType("geometry(Polygon, 4326)");
        
            entity.HasIndex(e => e.Location).HasMethod("GIST");
        });

        builder.Entity<PollutantInfo>().HasData(
            new PollutantInfo
            {
                Id = 1, Code = 2, Name = "Твердые частицы", ShortName = "PM2.5", Pollutant = Pollutant.SP,
                Mass = 15.72f, MaxPermissibleConcentration = 0.5f
            },
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
                Id = 5,
                Code = 304,
                Name = "Азота оксид (азот (II) оксид; азот монооксид)",
                ShortName = "NO",
                Pollutant = Pollutant.NO,
                SpecificEmission = 0.0182f,
                Mass = 0.0444f,
                MaxPermissibleConcentration = 0.4f,
                DailyAverageConcentration = 0.06f
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
                Id = 7,
                Code = 2754,
                Name = "Углеводороды предельные C12 - C19 (растворители РПК-240, РПК-280)",
                ShortName = "углеводороды",
                Pollutant = Pollutant.RPK240280,
                SpecificEmission = 99.72f,
                MaxPermissibleConcentration = 1f
            },
            new PollutantInfo
            {
                Id = 8,
                Code = 333,
                Name = "Сероводород (дигидросульфид; водород сернистый; гидросульфид)",
                ShortName = "дигидросульфид",
                Pollutant = Pollutant.H2S,
                SpecificEmission = 0.28f,
                MaxPermissibleConcentration = 0.008f
            },
            new PollutantInfo
            {
                Id = 9,
                Code = 123,
                Name = "диЖелезо триоксид (железа оксид; железо сесквиоксид) /в пересчете на железо/",
                ShortName = "Fe203",
                Pollutant = Pollutant.Fe2O3,
                DailyAverageConcentration = 0.04f
            },
            new PollutantInfo
            {
                Id = 10,
                Code = 143,
                Name = "Марганец и его соединения /в пересчете на марганец (IV) оксид/",
                ShortName = "MnO2",
                Pollutant = Pollutant.MnO2,
                MaxPermissibleConcentration = 0.01f,
                DailyAverageConcentration = 0.001f
            },
            new PollutantInfo
            {
                Id = 11,
                Code = 342,
                Name = "Фториды газообразные /в пересчете на фтор/: гидрофторид (водород фторид, фторводород); кремний тетрафторид",
                ShortName = "FluorideGases",
                Pollutant = Pollutant.FluorideGases,
                MaxPermissibleConcentration = 0.02f,
                DailyAverageConcentration = 0.005f
            },
            new PollutantInfo
            {
                Id = 12,
                Code = 380,
                Name = "Углерод диоксид",
                ShortName = "CO2",
                Pollutant = Pollutant.CO2,
                Mass = 4.9f,
                MaxPermissibleConcentration = 5f
            },
            new PollutantInfo
            {
                Id = 13,
                Code = 2,
                Name = "Твердые частицы",
                ShortName = "SP",
                Pollutant = Pollutant.SP,
                Mass = 15.72f,
                MaxPermissibleConcentration = 0.5f
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
            },
            new PollutantInfo
            {
                Id = 18,
                Code = 3749,
                Name = "Пыль каменного угля",
                ShortName = "CoalDust",
                Pollutant = Pollutant.CoalDust,
            }
        );

        builder.Entity<VaporConcentration>().HasData(
            new VaporConcentration
            {
                Id = 1, ReservoirType = ReservoirType.Ground, ClimateZone = ClimateZone.First, OilProduct = OilProduct.AutomobileGasoline,
                MaxVaporConcentration = 464f, AutumnWinterVaporConcentration = 205f, SpringSummerVaporConcentration = 248f
            },
            new VaporConcentration
            {
                Id = 2, ReservoirType = ReservoirType.Ground, ClimateZone = ClimateZone.First, OilProduct = OilProduct.DieselFuel,
                MaxVaporConcentration = 1.49f, AutumnWinterVaporConcentration = 0.79f, SpringSummerVaporConcentration = 1.06f
            },
            new VaporConcentration
            {
                Id = 3, ReservoirType = ReservoirType.Ground, ClimateZone = ClimateZone.First, OilProduct = OilProduct.Oils,
                MaxVaporConcentration = 0.16f, AutumnWinterVaporConcentration = 0.1f, SpringSummerVaporConcentration = 0.1f
            },
            new VaporConcentration
            {
                Id = 4, ReservoirType = ReservoirType.Ground, ClimateZone = ClimateZone.Second, OilProduct = OilProduct.AutomobileGasoline,
                MaxVaporConcentration = 580f, AutumnWinterVaporConcentration = 250f, SpringSummerVaporConcentration = 310f
            },
            new VaporConcentration
            {
                Id = 5, ReservoirType = ReservoirType.Ground, ClimateZone = ClimateZone.Second, OilProduct = OilProduct.DieselFuel,
                MaxVaporConcentration = 1.86f, AutumnWinterVaporConcentration = 0.96f, SpringSummerVaporConcentration = 1.32f
            },
            new VaporConcentration
            {
                Id = 6, ReservoirType = ReservoirType.Ground, ClimateZone = ClimateZone.Second, OilProduct = OilProduct.Oils,
                MaxVaporConcentration = 0.2f, AutumnWinterVaporConcentration = 0.12f, SpringSummerVaporConcentration = 0.12f
            },
            new VaporConcentration
            {
                Id = 7, ReservoirType = ReservoirType.Ground, ClimateZone = ClimateZone.Third, OilProduct = OilProduct.AutomobileGasoline,
                MaxVaporConcentration = 701.8f, AutumnWinterVaporConcentration = 310f, SpringSummerVaporConcentration = 375.1f
            },
            new VaporConcentration
            {
                Id = 8, ReservoirType = ReservoirType.Ground, ClimateZone = ClimateZone.Third, OilProduct = OilProduct.DieselFuel,
                MaxVaporConcentration = 2.25f, AutumnWinterVaporConcentration = 1.19f, SpringSummerVaporConcentration = 1.6f
            },
            new VaporConcentration
            {
                Id = 9, ReservoirType = ReservoirType.Ground, ClimateZone = ClimateZone.Third, OilProduct = OilProduct.Oils,
                MaxVaporConcentration = 0.24f, AutumnWinterVaporConcentration = 0.15f, SpringSummerVaporConcentration = 0.15f
            },
            new VaporConcentration
            {
                Id = 10, ReservoirType = ReservoirType.Buried, ClimateZone = ClimateZone.First, OilProduct = OilProduct.AutomobileGasoline,
                MaxVaporConcentration = 384f, AutumnWinterVaporConcentration = 172.2f, SpringSummerVaporConcentration = 255f
            },
            new VaporConcentration
            {
                Id = 11, ReservoirType = ReservoirType.Buried, ClimateZone = ClimateZone.First, OilProduct = OilProduct.DieselFuel,
                MaxVaporConcentration = 1.24f, AutumnWinterVaporConcentration = 0.66f, SpringSummerVaporConcentration = 0.88f
            },
            new VaporConcentration
            {
                Id = 12, ReservoirType = ReservoirType.Buried, ClimateZone = ClimateZone.First, OilProduct = OilProduct.Oils,
                MaxVaporConcentration = 0.13f, AutumnWinterVaporConcentration = 0.08f, SpringSummerVaporConcentration = 0.08f
            },
            new VaporConcentration
            {
                Id = 13, ReservoirType = ReservoirType.Buried, ClimateZone = ClimateZone.Second, OilProduct = OilProduct.AutomobileGasoline,
                MaxVaporConcentration = 480f, AutumnWinterVaporConcentration = 210.2f, SpringSummerVaporConcentration = 255f
            },
            new VaporConcentration
            {
                Id = 14, ReservoirType = ReservoirType.Buried, ClimateZone = ClimateZone.Second, OilProduct = OilProduct.DieselFuel,
                MaxVaporConcentration = 1.55f, AutumnWinterVaporConcentration = 0.8f, SpringSummerVaporConcentration = 1.1f
            },
            new VaporConcentration
            {
                Id = 15, ReservoirType = ReservoirType.Buried, ClimateZone = ClimateZone.Second, OilProduct = OilProduct.Oils,
                MaxVaporConcentration = 0.16f, AutumnWinterVaporConcentration = 0.1f, SpringSummerVaporConcentration = 0.1f
            },
            new VaporConcentration
            {
                Id = 16, ReservoirType = ReservoirType.Buried, ClimateZone = ClimateZone.Third, OilProduct = OilProduct.AutomobileGasoline,
                MaxVaporConcentration = 508f, AutumnWinterVaporConcentration = 260.4f, SpringSummerVaporConcentration = 308.5f
            },
            new VaporConcentration
            {
                Id = 17, ReservoirType = ReservoirType.Buried, ClimateZone = ClimateZone.Third, OilProduct = OilProduct.DieselFuel,
                MaxVaporConcentration = 1.88f, AutumnWinterVaporConcentration = 0.99f, SpringSummerVaporConcentration = 1.33f
            },
            new VaporConcentration
            {
                Id = 18, ReservoirType = ReservoirType.Buried, ClimateZone = ClimateZone.Third, OilProduct = OilProduct.Oils,
                MaxVaporConcentration = 0.19f, AutumnWinterVaporConcentration = 0.12f, SpringSummerVaporConcentration = 0.12f
            }
        );
    }
}