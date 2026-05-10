using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WeatherApi.Models.DTOs;
using WeatherApi.Services;

namespace WeatherApi.Controllers;

/// <summary>
/// Authentication Controller for JWT token generation
/// </summary>
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
    /// Login and receive JWT token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT token</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            // In production, validate against database with hashed passwords
            // This is a simplified example
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Username and password are required" });
            }

            // Simulate user validation (replace with actual database lookup)
            var isValidUser = ValidateUser(request.Username, request.Password, out var role);
            
            if (!isValidUser)
            {
                await _auditLogger.LogAsync(new Models.AuditLog
                {
                    UserId = request.Username,
                    Action = "LoginFailed",
                    Resource = "Auth/Login",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Details = "Invalid credentials"
                });

                return Unauthorized(new { message = "Invalid credentials" });
            }

            var token = GenerateJwtToken(request.Username, role);

            await _auditLogger.LogAsync(new Models.AuditLog
            {
                UserId = request.Username,
                Action = "LoginSuccess",
                Resource = "Auth/Login",
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            });

            return Ok(new LoginResponse
            {
                Token = token,
                Username = request.Username,
                Role = role,
                ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:ExpirationMinutes"]))
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
            throw;
        }
    }

    private bool ValidateUser(string username, string password, out string role)
    {
        // Simplified validation - replace with database lookup and password hashing
        // Example users:
        // admin/admin123 -> Admin role
        // user/user123 -> User role
        
        role = string.Empty;
        
        if (username == "admin" && password == "admin123")
        {
            role = "Admin";
            return true;
        }
        
        if (username == "user" && password == "user123")
        {
            role = "User";
            return true;
        }

        return false;
    }

    private string GenerateJwtToken(string username, string role)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, username),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(jwtSettings["ExpirationMinutes"])),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}