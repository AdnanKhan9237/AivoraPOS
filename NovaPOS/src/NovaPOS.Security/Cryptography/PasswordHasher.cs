using NovaPOS.Core.Interfaces.Security;

namespace NovaPOS.Security.Cryptography;

public class PasswordHasher : IPasswordHasher
{
  public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

  public bool VerifyPassword(string password, string passwordHash) =>
      BCrypt.Net.BCrypt.Verify(password, passwordHash);

  public string HashPin(string pin) => BCrypt.Net.BCrypt.HashPassword(pin, workFactor: 10);

  public bool VerifyPin(string pin, string pinHash) => BCrypt.Net.BCrypt.Verify(pin, pinHash);
}
