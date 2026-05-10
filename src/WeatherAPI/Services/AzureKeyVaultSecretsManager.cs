using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace WeatherAPI.Services;

/// <summary>
/// Azure Key Vault integration for secrets management
/// </summary>
public class AzureKeyVaultSecretsManager : ISecretsManager
{
    private readonly SecretClient? _secretClient;
    private readonly ILogger<AzureKeyVaultSecretsManager> _logger;
    private readonly bool _isEnabled;

    public AzureKeyVaultSecretsManager(IConfiguration configuration, ILogger<AzureKeyVaultSecretsManager> logger)
    {
        _logger = logger;
        var vaultUri = configuration["AzureKeyVault:VaultUri"];

        if (!string.IsNullOrEmpty(vaultUri) && Uri.TryCreate(vaultUri, UriKind.Absolute, out var uri))
        {
            try
            {
                _secretClient = new SecretClient(uri, new DefaultAzureCredential());
                _isEnabled = true;
                _logger.LogInformation("Azure Key Vault client initialized");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize Azure Key Vault client. Using local configuration.");
                _isEnabled = false;
            }
        }
        else
        {
            _logger.LogInformation("Azure Key Vault not configured. Using local configuration.");
            _isEnabled = false;
        }
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        if (!_isEnabled || _secretClient == null)
        {
            throw new InvalidOperationException("Azure Key Vault is not configured");
        }

        try
        {
            var secret = await _secretClient.GetSecretAsync(secretName);
            return secret.Value.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secret {SecretName} from Key Vault", secretName);
            throw;
        }
    }

    public async Task SetSecretAsync(string secretName, string secretValue)
    {
        if (!_isEnabled || _secretClient == null)
        {
            throw new InvalidOperationException("Azure Key Vault is not configured");
        }

        try
        {
            await _secretClient.SetSecretAsync(secretName, secretValue);
            _logger.LogInformation("Secret {SecretName} stored in Key Vault", secretName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store secret {SecretName} in Key Vault", secretName);
            throw;
        }
    }
}