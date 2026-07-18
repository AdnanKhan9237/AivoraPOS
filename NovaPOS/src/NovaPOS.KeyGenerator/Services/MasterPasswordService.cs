using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NovaPOS.KeyGenerator.Data;

namespace NovaPOS.KeyGenerator.Services;

public sealed class MasterPasswordService
{
    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "NovaPOS",
        "KeyGenerator",
        "auth.json");

    public bool IsConfigured => File.Exists(ConfigPath);

    public void SetMasterPassword(string password)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
        var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        var json = JsonSerializer.Serialize(new AuthConfig { PasswordHash = hash });
        File.WriteAllText(ConfigPath, json);
    }

    public bool Verify(string password)
    {
        if (!File.Exists(ConfigPath))
        {
            return false;
        }

        var json = File.ReadAllText(ConfigPath);
        var config = JsonSerializer.Deserialize<AuthConfig>(json);
        return config is not null && BCrypt.Net.BCrypt.Verify(password, config.PasswordHash);
    }

    private sealed class AuthConfig
    {
        public string PasswordHash { get; set; } = string.Empty;
    }
}
