using System.Security.Cryptography;
using System.Text;

namespace WeatherAPI.Security;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;
    private readonly ILogger<EncryptionService> _logger;

    public EncryptionService(IConfiguration configuration, ILogger<EncryptionService> logger)
    {
        _logger = logger;
        var keyString = configuration["Encryption:Key"] ?? throw new InvalidOperationException("Encryption key not configured");
        var ivString = configuration["Encryption:IV"] ?? throw new InvalidOperationException("Encryption IV not configured");
        
        _key = Encoding.UTF8.GetBytes(keyString);
        _iv = Encoding.UTF8.GetBytes(ivString);

        if (_key.Length != 32)
        {
            throw new InvalidOperationException("Encryption key must be 32 bytes for AES-256");
        }

        if (_iv.Length != 16)
        {
            throw new InvalidOperationException("Encryption IV must be 16 bytes");
        }
    }

    public string Encrypt(string plainText)
    {
        try
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            
            return Convert.ToBase64String(cipherBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting data");
            throw;
        }
    }

    public string Decrypt(string cipherText)
    {
        try
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return cipherText;
            }

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var cipherBytes = Convert.FromBase64String(cipherText);
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting data");
            throw;
        }
    }

    public byte[] EncryptBytes(byte[] plainBytes)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting bytes");
            throw;
        }
    }

    public byte[] DecryptBytes(byte[] cipherBytes)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting bytes");
            throw;
        }
    }
}