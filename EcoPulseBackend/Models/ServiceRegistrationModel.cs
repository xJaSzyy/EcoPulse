namespace EcoPulseBackend.Models;

public class ServiceRegistrationModel
{
    public string ServiceName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string[]? Tags { get; set; }
    public string CheckInterval { get; set; } = null!;
}