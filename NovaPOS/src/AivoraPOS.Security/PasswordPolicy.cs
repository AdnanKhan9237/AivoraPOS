using System.Text.RegularExpressions;

namespace AivoraPOS.Security;

public static partial class PasswordPolicy
{
    private static readonly HashSet<string> BlockedPins = new(StringComparer.Ordinal)
    {
        "0000", "1111", "1234", "2222", "3333", "4444",
        "5555", "6666", "7777", "8888", "9999", "1212", "4321"
    };

    public static bool IsPinAllowed(string pin) =>
        pin.Length == 4 && pin.All(char.IsDigit) && !BlockedPins.Contains(pin);

    public static bool IsPasswordValid(string password, out string errorMessage)
    {
        if (password.Length < 8)
        {
            errorMessage = "Password must be at least 8 characters.";
            return false;
        }

        if (!password.Any(char.IsUpper))
        {
            errorMessage = "Password must contain at least one uppercase letter.";
            return false;
        }

        if (!password.Any(char.IsDigit))
        {
            errorMessage = "Password must contain at least one number.";
            return false;
        }

        if (!SpecialCharacterRegex().IsMatch(password))
        {
            errorMessage = "Password must contain at least one special character.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    [GeneratedRegex(@"[!@#$%^&*(),.?""':{}|<>_\-\+=\[\]\\;/]")]
    private static partial Regex SpecialCharacterRegex();
}
