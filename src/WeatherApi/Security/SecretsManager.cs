using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace WeatherApi.Security;

/// <summary>
/// Azure Key Vault secrets manager for secure credential storage
/// </summary>
public class SecretsManager : ISecretsManager
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecretsManager> _logger;
    private SecretClient? _secretClient;

    public SecretsManager(IConfiguration configuration, ILogger<SecretsManager> logger)
    {
        _configuration = configuration;
        _logger = logger;
        InitializeKeyVaultClient();
    }

    private void InitializeKeyVaultClient()
    {
        try
        {
            var keyVaultUrl = _configuration["AzureKeyVault:VaultUrl"];
            
            if (!string.IsNullOrEmpty(keyVaultUrl))
            {
                _secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
                _logger.LogInformation("Azure Key Vault client initialized");
            }
            else
            {
                _logger.LogWarning("Azure Key Vault URL not configured. Using local configuration.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Azure Key Vault client");
        }
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        try
        {
            if (_secretClient != null)
            {
                var secret = await _secretClient.GetSecretAsync(secretName);
                return secret.Value.Value;
            }
            else
            {
                // Fallback to configuration
                var value = _configuration[secretName];
                if (string.IsNullOrEmpty(value))
                {
                    throw new InvalidOperationException($"Secret {secretName} not found in configuration");
                }
                return value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving secret: {SecretName}", secretName);
            throw;
        }
    }

    public async Task SetSecretAsync(string secretName, string secretValue)
    {
        try
        {
            if (_secretClient != null)
            {
                await _secretClient.SetSecretAsync(secretName, secretValue);
                _logger.LogInformation("Secret {SecretName} updated in Key Vault", secretName);
            }
            else
            {
                _logger.LogWarning("Key Vault not configured. Secret {SecretName} not stored.", secretName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting secret: {SecretName}", secretName);
            throw;
        }
    }
}