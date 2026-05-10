namespace WeatherAPI.Services;

public interface ISecretsManager
{
    Task<string> GetSecretAsync(string secretName);
    Task SetSecretAsync(string secretName, string secretValue);
}