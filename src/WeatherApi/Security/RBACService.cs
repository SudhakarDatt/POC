namespace WeatherApi.Security;

/// <summary>
/// Role-Based Access Control (RBAC) service
/// </summary>
public class RBACService : IRBACService
{
    private readonly ILogger<RBACService> _logger;
    
    // In production, this would be stored in database
    private readonly Dictionary<string, List<string>> _rolePermissions = new()
    {
        { "Admin", new List<string> { "weather.read", "weather.write", "weather.delete", "audit.read", "statistics.read" } },
        { "User", new List<string> { "weather.read" } },
        { "PowerUser", new List<string> { "weather.read", "weather.write", "statistics.read" } }
    };

    private readonly Dictionary<string, List<string>> _userRoles = new()
    {
        { "admin", new List<string> { "Admin" } },
        { "user", new List<string> { "User" } }
    };

    public RBACService(ILogger<RBACService> logger)
    {
        _logger = logger;
    }

    public Task<bool> HasPermissionAsync(string userId, string permission)
    {
        try
        {
            if (!_userRoles.TryGetValue(userId, out var roles))
            {
                _logger.LogWarning("User {UserId} has no roles assigned", userId);
                return Task.FromResult(false);
            }

            foreach (var role in roles)
            {
                if (_rolePermissions.TryGetValue(role, out var permissions))
                {
                    if (permissions.Contains(permission))
                    {
                        return Task.FromResult(true);
                    }
                }
            }

            _logger.LogWarning("User {UserId} does not have permission: {Permission}", userId, permission);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission for user: {UserId}", userId);
            return Task.FromResult(false);
        }
    }

    public Task<IEnumerable<string>> GetUserPermissionsAsync(string userId)
    {
        try
        {
            var allPermissions = new HashSet<string>();

            if (_userRoles.TryGetValue(userId, out var roles))
            {
                foreach (var role in roles)
                {
                    if (_rolePermissions.TryGetValue(role, out var permissions))
                    {
                        foreach (var permission in permissions)
                        {
                            allPermissions.Add(permission);
                        }
                    }
                }
            }

            return Task.FromResult<IEnumerable<string>>(allPermissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions for user: {UserId}", userId);
            return Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
        }
    }

    public Task<bool> AssignRoleAsync(string userId, string role)
    {
        try
        {
            if (!_rolePermissions.ContainsKey(role))
            {
                _logger.LogWarning("Role {Role} does not exist", role);
                return Task.FromResult(false);
            }

            if (!_userRoles.ContainsKey(userId))
            {
                _userRoles[userId] = new List<string>();
            }

            if (!_userRoles[userId].Contains(role))
            {
                _userRoles[userId].Add(role);
                _logger.LogInformation("Role {Role} assigned to user {UserId}", role, userId);
            }

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {Role} to user {UserId}", role, userId);
            return Task.FromResult(false);
        }
    }

    public Task<bool> RevokeRoleAsync(string userId, string role)
    {
        try
        {
            if (_userRoles.TryGetValue(userId, out var roles))
            {
                if (roles.Remove(role))
                {
                    _logger.LogInformation("Role {Role} revoked from user {UserId}", role, userId);
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking role {Role} from user {UserId}", role, userId);
            return Task.FromResult(false);
        }
    }
}