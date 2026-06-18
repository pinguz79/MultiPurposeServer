using Microsoft.AspNetCore.Mvc;

namespace MultiPurposeServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "Healthy", time = DateTime.UtcNow });
}

