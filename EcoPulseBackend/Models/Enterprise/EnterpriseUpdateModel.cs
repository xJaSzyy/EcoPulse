using NetTopologySuite.Geometries;

namespace EcoPulseBackend.Models.Enterprise;

public class EnterpriseUpdateModel
{
    public int Id { get; set; }

    public string? Name { get; set; } = null!;
    
    public Polygon? SanitaryArea { get; set; } = null!;
}