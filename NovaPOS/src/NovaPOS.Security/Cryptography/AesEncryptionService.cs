using System.Security.Cryptography;
using System.Text;
using NovaPOS.Core.Interfaces.Security;

namespace NovaPOS.Security.Cryptography;

public class AesEncryptionService : IEncryptionService
{
    private const int KeySizeBytes = 32;
    private const int IvSizeBytes = 16;

    private readonly byte[] _masterKey;

    public AesEncryptionService()
    {
        _masterKey = DeriveMachineKey();
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _masterKey;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var payload = new byte[IvSizeBytes + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, payload, 0, IvSizeBytes);
        Buffer.BlockCopy(cipherBytes, 0, payload, IvSizeBytes, cipherBytes.Length);

        return Convert.ToBase64String(payload);
    }

    public string Decrypt(string cipherText)
    {
        var payload = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _masterKey;

        var iv = new byte[IvSizeBytes];
        var cipherBytes = new byte[payload.Length - IvSizeBytes];
        Buffer.BlockCopy(payload, 0, iv, 0, IvSizeBytes);
        Buffer.BlockCopy(payload, IvSizeBytes, cipherBytes, 0, cipherBytes.Length);

        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }

    public string GenerateKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(KeySizeBytes);
        return Convert.ToBase64String(bytes);
    }

    private static byte[] DeriveMachineKey()
    {
        var entropy = $"{Environment.MachineName}|{Environment.UserName}|NovaPOS|v1";
        return SHA256.HashData(Encoding.UTF8.GetBytes(entropy));
    }
}
