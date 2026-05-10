using Microsoft.EntityFrameworkCore;
using WeatherAPI.Data;

namespace WeatherAPI.Security;

/// <summary>
/// Role-Based Access Control (RBAC) service
/// </summary>
public class RBACService : IRBACService
{
    private readonly WeatherDbContext _context;
    private readonly ILogger<RBACService> _logger;

    // Define role-permission mappings
    private readonly Dictionary<string, List<string>> _rolePermissions = new()
    {
        { "Admin", new List<string> { "weather.read", "weather.write", "weather.delete", "alert.create", "alert.delete", "user.manage" } },
        { "User", new List<string> { "weather.read", "alert.read" } },
        { "Guest", new List<string> { "weather.read" } }
    };

    public RBACService(WeatherDbContext context, ILogger<RBACService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(string userId, string permission)
    {
        try
        {
            var roles = await GetUserRolesAsync(userId);
            
            foreach (var role in roles)
            {
                if (_rolePermissions.TryGetValue(role, out var permissions))
                {
                    if (permissions.Contains(permission))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission for user {UserId}", userId);
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
    {
        // In production, retrieve from database or identity provider
        // For demo, return default roles
        await Task.CompletedTask;
        
        // Default role assignment logic
        if (userId.Contains("admin", StringComparison.OrdinalIgnoreCase))
        {
            return new[] { "Admin", "User" };
        }
        
        return new[] { "User" };
    }

    public async Task<IEnumerable<string>> GetRolePermissionsAsync(string role)
    {
        await Task.CompletedTask;
        
        if (_rolePermissions.TryGetValue(role, out var permissions))
        {
            return permissions;
        }
        
        return Enumerable.Empty<string>();
    }

    public async Task AssignRoleAsync(string userId, string role)
    {
        // Implementation would store role assignment in database
        await Task.CompletedTask;
        _logger.LogInformation("Assigned role {Role} to user {UserId}", role, userId);
    }

    public async Task RevokeRoleAsync(string userId, string role)
    {
        // Implementation would remove role assignment from database
        await Task.CompletedTask;
        _logger.LogInformation("Revoked role {Role} from user {UserId}", role, userId);
    }
}