using AivoraPOS.Core;

namespace AivoraPOS.Core.Tests;

public class AppVersionTests
{
    [Theory]
    [InlineData("1.1.0", "1.0.0", true)]
    [InlineData("1.0.1", "1.0.0", true)]
    [InlineData("2.0.0", "1.9.9", true)]
    [InlineData("1.0.0", "1.0.0", false)]
    [InlineData("0.9.0", "1.0.0", false)]
    [InlineData("v1.2.0", "1.1.0", true)]
    public void IsNewer_ComparesSemanticVersions(string remote, string local, bool expected)
    {
        Assert.Equal(expected, AppVersion.IsNewer(remote, local));
    }

    [Fact]
    public void Current_ReturnsNonEmptyVersion()
    {
        Assert.False(string.IsNullOrWhiteSpace(AppVersion.Current));
    }
}
