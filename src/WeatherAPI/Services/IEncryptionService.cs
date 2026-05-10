namespace WeatherAPI.Services;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    byte[] GenerateKey();
    byte[] GenerateIV();
}