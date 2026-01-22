using EcoPulseBackend.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EcoPulseBackend;

public static class DependencyInjection
{
    public static void AddServices(this IServiceCollection services)
    {
    }

    public static void AddDatabase(this IServiceCollection services, ConfigurationManager configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
    }
}