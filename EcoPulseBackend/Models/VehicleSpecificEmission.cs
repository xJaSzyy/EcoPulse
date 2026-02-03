using System.ComponentModel.DataAnnotations;
using EcoPulseBackend.Enums;

namespace EcoPulseBackend.Models;

public class VehicleSpecificEmission
{
    [Key]
    public int Id { get; set; }
    
    public VehicleType VehicleType { get; set; }
    
    public Pollutant Pollutant { get; set; }
    
    public float SpecificEmission { get; set; }
}