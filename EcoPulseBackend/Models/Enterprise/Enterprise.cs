using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Models.Enterprise;

public class Enterprise
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    
    public Polygon SanitaryArea { get; set; } = null!;

    public List<SingleEmissionSource.SingleEmissionSource> SingleEmissionSources { get; set; } = new();
}