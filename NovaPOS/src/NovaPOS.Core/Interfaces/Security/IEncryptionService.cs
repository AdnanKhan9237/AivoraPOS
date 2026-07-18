namespace NovaPOS.Core.Interfaces.Security;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    string GenerateKey();
}
