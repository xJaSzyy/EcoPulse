using EcoPulseBackend.Interfaces;

namespace EcoPulseBackend.Services;

public class EmissionService : IEmissionService
{
    public IMaximumSingleService MaximumSingleService { get; }
    
    public EmissionService(IMaximumSingleService maximumSingleService)
    {
        MaximumSingleService = maximumSingleService;
    }
}