namespace AivoraPOS.Core.Interfaces.Security;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
    string HashPin(string pin);
    bool VerifyPin(string pin, string pinHash);
}
