namespace WeatherApi.Security;

public interface ISecretsManager
{
    Task<string> GetSecretAsync(string secretName);
    Task SetSecretAsync(string secretName, string secretValue);
}