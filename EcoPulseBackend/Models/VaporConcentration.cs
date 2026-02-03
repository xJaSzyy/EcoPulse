using System.ComponentModel.DataAnnotations;
using EcoPulseBackend.Enums;

namespace EcoPulseBackend.Models;

public class VaporConcentration
{
    [Key]
    public int Id { get; set; }
    
    public ReservoirType ReservoirType { get; set; }
    
    public ClimateZone ClimateZone { get; set; }
    
    public OilProduct OilProduct { get; set; }
    
    public float MaxVaporConcentration { get; set; }
    
    public float AutumnWinterVaporConcentration { get; set; }
    
    public float SpringSummerVaporConcentration { get; set; }
}