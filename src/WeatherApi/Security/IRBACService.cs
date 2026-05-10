namespace WeatherApi.Security;

public interface IRBACService
{
    Task<bool> HasPermissionAsync(string userId, string permission);
    Task<IEnumerable<string>> GetUserPermissionsAsync(string userId);
    Task<bool> AssignRoleAsync(string userId, string role);
    Task<bool> RevokeRoleAsync(string userId, string role);
}