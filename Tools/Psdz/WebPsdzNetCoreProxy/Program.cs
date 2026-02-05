using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

// Add YARP reverse proxy services
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Health check endpoint
app.MapHealthChecks("/health");

// Minimal API endpoints
app.MapGet("/api/v2/status", () => Results.Ok(new
{
    Status = "Running",
    Version = "2.0",
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName
}))
.WithName("GetStatus")
.WithTags("Status");

app.MapGet("/api/v2/info", () => Results.Ok(new
{
    Application = "WebPsdzNetCoreProxy",
    Framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
    OS = System.Runtime.InteropServices.RuntimeInformation.OSDescription
}))
.WithName("GetInfo")
.WithTags("Info");

// Map controllers (for more complex endpoints)
app.MapControllers();

// Map the reverse proxy routes (catch-all for legacy app)
// This must be last so other endpoints take precedence
app.MapReverseProxy();

app.Run();
