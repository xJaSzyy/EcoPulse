namespace EcoPulseBackend.Models.DangerZone;

public class VehicleFlowDangerZoneCalculateModel
{
    /// <summary>
    /// Список идентификаторов городов
    /// </summary>
    public ICollection<int> CityIds { get; set; } = new List<int>();
}