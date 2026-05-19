using EcoPulseTrafficService;
using EcoPulseTrafficService.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddServices();
builder.Services.AddHttpClient();
builder.Services.AddHealthChecks();

builder.Services.Configure<TrafficServiceOptions>(
    builder.Configuration.GetSection("TrafficService"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync("OK");
    }
});
app.Run();