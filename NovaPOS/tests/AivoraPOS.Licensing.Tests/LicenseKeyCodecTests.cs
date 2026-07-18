using AivoraPOS.Core.Enums;
using AivoraPOS.Core.Models;
using AivoraPOS.Licensing;
using AivoraPOS.Licensing.Constants;

namespace AivoraPOS.Licensing.Tests;

public class LicenseKeyCodecTests
{
  private static readonly byte[] TestKey = LicenseConstants.DefaultVerificationKey;

  [Fact]
  public void Generate_And_Parse_RoundTrip_Succeeds()
  {
    var payload = new LicenseKeyPayload
    {
      Plan = LicensePlan.Professional,
      ExpiresAtUtc = DateTime.UtcNow.AddYears(1),
      Salt = 123456
    };

    var key = LicenseKeyCodec.Generate(payload, TestKey);

    Assert.StartsWith("NOVA-", key);
    Assert.True(LicenseKeyCodec.TryParse(key, TestKey, out var parsed));
    Assert.Equal(payload.Plan, parsed.Plan);
    Assert.Equal(payload.Salt, parsed.Salt);
  }

  [Fact]
  public void TryParse_RejectsTamperedKey()
  {
    var payload = new LicenseKeyPayload
    {
      Plan = LicensePlan.Starter,
      ExpiresAtUtc = DateTime.UtcNow.AddMonths(1),
      Salt = 42
    };

    var key = LicenseKeyCodec.Generate(payload, TestKey);
    var tampered = key.Replace('A', 'B');

    Assert.False(LicenseKeyCodec.TryParse(tampered, TestKey, out _));
  }
}
