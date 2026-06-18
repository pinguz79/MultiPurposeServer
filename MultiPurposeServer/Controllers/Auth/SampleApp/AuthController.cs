using Microsoft.AspNetCore.Mvc;
using MultiPurposeServer.Models.Auth;

namespace MultiPurposeServer.Controllers.Auth.SampleApp;

[ApiController]
[Route("Auth/SampleApp")]
public class AuthController : ControllerBase
{
    /// <summary>
    /// Login endpoint for SampleApp clients.
    /// </summary>
    /// <param name="request">Username/password payload.</param>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request is null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { error = "username and password are required" });

        // NOTE: Replace this simple check with proper user store and password hashing.
        if (request.Username == "sample" && request.Password == "password")
        {
            var response = new LoginResponse
            {
                Token = Guid.NewGuid().ToString(),
                Expires = DateTime.UtcNow.AddHours(1)
            };
            return Ok(response);
        }

        return Unauthorized(new { error = "invalid credentials" });
    }
}
