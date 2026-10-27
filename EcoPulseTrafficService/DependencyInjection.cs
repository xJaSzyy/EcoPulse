using EcoPulseTrafficService.Interfaces;
using EcoPulseTrafficService.Services;

namespace EcoPulseTrafficService;

public static class DependencyInjection
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<ITrafficService, TrafficService>();
        services.AddHostedService<TrafficHostedService>();
        services.AddHostedService<ConsulRegistrationService>();
    }
}