using NovaPOS.Core.Enums;
using NovaPOS.Core.Extensions;

namespace NovaPOS.Licensing.Tests;

public class LicensePlanExtensionsTests
{
  [Theory]
  [InlineData(LicensePlan.Starter, LicenseFeature.FullReports, false)]
  [InlineData(LicensePlan.Professional, LicenseFeature.FullReports, true)]
  [InlineData(LicensePlan.Enterprise, LicenseFeature.MultiCurrency, true)]
  [InlineData(LicensePlan.Professional, LicenseFeature.MultiCurrency, false)]
  public void Supports_ReturnsExpectedFeatureAccess(LicensePlan plan, LicenseFeature feature, bool expected)
  {
    Assert.Equal(expected, plan.Supports(feature));
  }

  [Fact]
  public void Starter_HasExpectedLimits()
  {
    Assert.Equal(2, LicensePlan.Starter.GetMaxCashiers());
    Assert.Equal(500, LicensePlan.Starter.GetMaxProducts());
  }
}
