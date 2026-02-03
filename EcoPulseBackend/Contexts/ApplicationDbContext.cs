using EcoPulseBackend.Enums;
using EcoPulseBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoPulseBackend.Contexts;

public class ApplicationDbContext : DbContext
{
    public virtual DbSet<PollutantInfo> PollutantInfos { get; set; } = null!;
    
    public ApplicationDbContext() {  }
    
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<PollutantInfo>().HasData(
            new PollutantInfo { Id = 1, Code = 2, Name = "Твердые частицы", ShortName = "PM2.5", Pollutant = Pollutant.SP, Mass = 15.72f, MaxPermissibleConcentration = 0.5f }
        );
    }
}