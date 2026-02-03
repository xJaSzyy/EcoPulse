namespace EcoPulseBackend.Models.Result;

public class EmissionsGroupResult
{
    public PollutantInfo PollutantInfo { get; set; } = null!;
    
    public List<EmissionsResult> Emissions { get; set; } = new();
}