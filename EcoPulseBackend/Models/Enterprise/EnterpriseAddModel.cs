using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Models.Enterprise;

public class EnterpriseAddModel
{
    public string Name { get; set; } = null!;
    
    public Polygon SanitaryArea { get; set; } = null!;
    
    public int CityId { get; set; }
}