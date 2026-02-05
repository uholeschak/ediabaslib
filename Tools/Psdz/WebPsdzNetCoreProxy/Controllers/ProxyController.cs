using Microsoft.AspNetCore.Mvc;

namespace WebPsdzNetCoreProxy.Controllers;

/// <summary>
/// New ASP.NET Core API endpoints that are handled natively by the proxy.
/// These endpoints bypass the legacy application.
/// </summary>
[ApiController]
[Route("api/v2/[controller]")]
public class ProxyController : ControllerBase
{
    private readonly ILogger<ProxyController> _logger;
    private readonly IConfiguration _configuration;

    public ProxyController(ILogger<ProxyController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Gets the current proxy configuration summary.
    /// </summary>
    [HttpGet("config")]
    public IActionResult GetConfig()
    {
        var clusters = _configuration.GetSection("ReverseProxy:Clusters").GetChildren()
            .Select(c => new
            {
                Name = c.Key,
                Destinations = c.GetSection("Destinations").GetChildren()
                    .Select(d => new
                    {
                        Name = d.Key,
                        Address = d["Address"]
                    })
            });

        return Ok(new
        {
            Timestamp = DateTime.UtcNow,
            Clusters = clusters
        });
    }

    /// <summary>
    /// Ping endpoint to verify the proxy is responsive.
    /// </summary>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        _logger.LogDebug("Ping received");
        return Ok(new { Message = "pong", Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Echo endpoint for testing request/response.
    /// </summary>
    [HttpPost("echo")]
    public IActionResult Echo([FromBody] object payload)
    {
        return Ok(new
        {
            ReceivedAt = DateTime.UtcNow,
            Payload = payload,
            Headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
        });
    }
}
