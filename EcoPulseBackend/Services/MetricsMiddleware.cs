using System.Diagnostics;

namespace EcoPulseBackend.Services;

public class MetricsMiddleware
{
    private readonly RequestDelegate _next;

    public MetricsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        await _next(context);

        sw.Stop();

        var endpoint = context.Request.Path.Value ?? "unknown";
        var method = context.Request.Method;

        MetricsService.HttpRequestsTotal
            .WithLabels(method, endpoint)
            .Inc();

        MetricsService.HttpRequestDuration
            .Observe(sw.Elapsed.TotalSeconds);
    }
}