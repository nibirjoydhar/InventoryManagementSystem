using Inventory.Application.DTOs.Auth;
using Inventory.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Inventory.Api.Controllers
{
    /// <summary>
    /// Controller for authentication endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Constructor for AuthController
        /// </summary>
        /// <param name="authService">Service handling authentication logic</param>
        /// <param name="logger">Logger instance for tracking events</param>
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Login endpoint for users
        /// </summary>
        /// <param name="dto">Login request data containing username and password</param>
        /// <returns>JWT token with expiration, username, and roles if successful; Unauthorized if failed</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.LoginAsync(dto);
                if (result == null)
                {
                    _logger.LogWarning(
                        "Failed login attempt for user {Username} at {Time}",
                        dto.Username, DateTime.UtcNow);
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                _logger.LogInformation(
                    "User {Username} logged in successfully at {Time}",
                    dto.Username, DateTime.UtcNow);

                return Ok(new
                {
                    Token = result.Token,
                    ExpiresIn = result.ExpiresIn,
                    Username = result.Username,
                    Roles = result.Roles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred during login for user {Username}",
                    dto.Username);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
