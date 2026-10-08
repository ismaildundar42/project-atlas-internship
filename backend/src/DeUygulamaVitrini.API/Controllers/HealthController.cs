using Microsoft.AspNetCore.Mvc;

namespace DeUygulamaVitrini.API.Controllers;

/// <summary>
/// Uygulamanın çalışır durumda olduğunu doğrulayan sağlık kontrolü endpoint'i.
/// Load balancer, container orchestration ve frontend health check'leri tarafından kullanılabilir.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Uygulamanın temel sağlık durumunu döner.
    /// GET /api/health
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var response = new HealthResponse(
            Status: "Healthy",
            Timestamp: DateTime.UtcNow,
            Version: "1.0.0-phase1"
        );

        return Ok(response);
    }
}

/// <summary>
/// Health endpoint yanıt modeli.
/// Record türü kullanılarak immutable ve sade bir DTO oluşturulmuştur.
/// </summary>
public record HealthResponse(
    string Status,
    DateTime Timestamp,
    string Version
);
