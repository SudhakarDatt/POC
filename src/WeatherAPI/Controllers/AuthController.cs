using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WeatherAPI.Models.DTOs;
using WeatherAPI.Services;

namespace WeatherAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IConfiguration configuration,
        IAuditLogger auditLogger,
        ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO: Validate credentials against your user store
            // This is a simplified example
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                await _auditLogger.LogAsync(new Models.AuditLog
                {
                    UserId = request.Username,
                    Action = "Login",
                    Resource = "Auth",
                    Result = "Failed",
                    Timestamp = DateTime.UtcNow,
                    Details = "Invalid credentials"
                });
                return Unauthorized("Invalid credentials");
            }

            // Generate JWT token
            var token = GenerateJwtToken(request.Username, new[] { "User" });

            await _auditLogger.LogAsync(new Models.AuditLog
            {
                UserId = request.Username,
                Action = "Login",
                Resource = "Auth",
                Result = "Success",
                Timestamp = DateTime.UtcNow
            });

            return Ok(new AuthResponse
            {
                Token = token,
                ExpiresIn = 3600,
                TokenType = "Bearer"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication");
            return StatusCode(500, "An error occurred during authentication");
        }
    }

    private string GenerateJwtToken(string username, string[] roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, username)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationMinutes"])),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}