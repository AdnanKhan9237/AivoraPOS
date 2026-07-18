using System.Security.Cryptography;
using System.Text;
using AivoraPOS.Core.Enums;
using AivoraPOS.Core.Models;
using AivoraPOS.Licensing.Constants;

namespace AivoraPOS.Licensing;

public static class LicenseKeyCodec
{
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public static string Generate(LicenseKeyPayload payload, byte[] signingKey)
    {
        var bytes = EncodePayload(payload, signingKey);
        var encoded = Base32Encode(bytes);

        return FormatKey(encoded);
    }

    public static bool TryParse(string licenseKey, byte[] verificationKey, out LicenseKeyPayload payload)
    {
        payload = null!;

        if (!TryNormalizeKey(licenseKey, out var encoded))
        {
            return false;
        }

        if (!TryBase32Decode(encoded, out var bytes) || bytes.Length != 10)
        {
            return false;
        }

        if (!LicenseValidator.VerifySignature(bytes, verificationKey))
        {
            return false;
        }

        var planValue = bytes[0];
        if (!Enum.IsDefined(typeof(LicensePlan), (int)planValue))
        {
            return false;
        }

        var expiryUnix = ReadUInt32BigEndian(bytes, 1);
        var salt = ReadUInt32BigEndian(bytes, 5);

        payload = new LicenseKeyPayload
        {
            Plan = (LicensePlan)planValue,
            ExpiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(expiryUnix).UtcDateTime,
            Salt = salt
        };

        return true;
    }

    public static byte[] EncodePayload(LicenseKeyPayload payload, byte[] signingKey)
    {
        var bytes = new byte[10];
        bytes[0] = (byte)payload.Plan;
        WriteUInt32BigEndian(bytes, 1, (uint)payload.ExpiresAtUtc.Subtract(DateTime.UnixEpoch).TotalSeconds);
        WriteUInt32BigEndian(bytes, 5, payload.Salt);
        bytes[9] = LicenseValidator.ComputeSignatureByte(bytes.AsSpan(0, 9), signingKey);
        return bytes;
    }

    public static string FormatKey(string encoded)
    {
        if (encoded.Length != 16)
        {
            throw new ArgumentException("Encoded key must be 16 characters.", nameof(encoded));
        }

        return $"{LicenseConstants.KeyPrefix}-{encoded[..4]}-{encoded[4..8]}-{encoded[8..12]}-{encoded[12..16]}";
    }

    private static bool TryNormalizeKey(string licenseKey, out string encoded)
    {
        encoded = string.Empty;
        var normalized = licenseKey.Trim().ToUpperInvariant().Replace(" ", string.Empty);

        if (!normalized.StartsWith($"{LicenseConstants.KeyPrefix}-", StringComparison.Ordinal))
        {
            return false;
        }

        var body = normalized[LicenseConstants.KeyPrefix.Length..].TrimStart('-');
        body = body.Replace("-", string.Empty);

        if (body.Length != 16)
        {
            return false;
        }

        encoded = body;
        return encoded.All(c => Base32Alphabet.Contains(c));
    }

    private static string Base32Encode(byte[] data)
    {
        var output = new StringBuilder((data.Length * 8 + 4) / 5);
        int buffer = 0, bitsLeft = 0;

        foreach (var b in data)
        {
            buffer = (buffer << 8) | b;
            bitsLeft += 8;

            while (bitsLeft >= 5)
            {
                var index = (buffer >> (bitsLeft - 5)) & 31;
                bitsLeft -= 5;
                output.Append(Base32Alphabet[index]);
            }
        }

        if (bitsLeft > 0)
        {
            var index = (buffer << (5 - bitsLeft)) & 31;
            output.Append(Base32Alphabet[index]);
        }

        return output.ToString().PadRight(16, Base32Alphabet[0])[..16];
    }

    private static bool TryBase32Decode(string input, out byte[] data)
    {
        data = Array.Empty<byte>();
        var bytes = new List<byte>();
        int buffer = 0, bitsLeft = 0;

        foreach (var c in input)
        {
            var index = Base32Alphabet.IndexOf(c);
            if (index < 0)
            {
                return false;
            }

            buffer = (buffer << 5) | index;
            bitsLeft += 5;

            if (bitsLeft >= 8)
            {
                bytes.Add((byte)(buffer >> (bitsLeft - 8)));
                bitsLeft -= 8;
            }
        }

        data = bytes.ToArray();
        return true;
    }

    private static void WriteUInt32BigEndian(byte[] buffer, int offset, uint value)
    {
        buffer[offset] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)value;
    }

    private static uint ReadUInt32BigEndian(byte[] buffer, int offset) =>
        ((uint)buffer[offset] << 24)
        | ((uint)buffer[offset + 1] << 16)
        | ((uint)buffer[offset + 2] << 8)
        | buffer[offset + 3];
}
