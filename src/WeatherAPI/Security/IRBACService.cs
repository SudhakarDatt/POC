namespace WeatherAPI.Security;

public interface IRBACService
{
    Task<bool> HasPermissionAsync(string userId, string permission);
    Task<IEnumerable<string>> GetUserRolesAsync(string userId);
    Task<IEnumerable<string>> GetRolePermissionsAsync(string role);
    Task AssignRoleAsync(string userId, string role);
    Task RevokeRoleAsync(string userId, string role);
}