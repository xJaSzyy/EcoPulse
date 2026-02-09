namespace EcoPulseBackend.Models.TileGrid;

public class TileGridResult
{
    public int CityId { get; set; }
    public List<TileModel> Tiles { get; set; } = new();
}