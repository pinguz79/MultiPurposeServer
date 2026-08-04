using Microsoft.AspNetCore.Mvc;

namespace MultiPurposeServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SampleController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(new { message = "MPS API running", version = "v1" });

        [HttpGet("echo")]
        public IActionResult Echo([FromQuery] string? text) => Ok(new { echo = text ?? string.Empty });
    }
}
