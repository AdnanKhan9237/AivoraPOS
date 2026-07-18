using System.Security.Cryptography;

namespace NovaPOS.Licensing;

public static class LicenseValidator
{
    public static bool VerifySignature(ReadOnlySpan<byte> payloadBytes, byte[] verificationKey)
    {
        if (payloadBytes.Length != 10)
        {
            return false;
        }

        var expected = ComputeSignatureByte(payloadBytes[..9], verificationKey);
        return payloadBytes[9] == expected;
    }

    public static byte ComputeSignatureByte(ReadOnlySpan<byte> payloadBytes, byte[] key)
    {
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(payloadBytes.ToArray());
        return hash[0];
    }

    public static bool IsExpired(DateTime expiresAtUtc) => expiresAtUtc <= DateTime.UtcNow;
}
