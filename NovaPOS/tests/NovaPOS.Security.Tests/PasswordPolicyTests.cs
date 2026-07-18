using NovaPOS.Security;

namespace NovaPOS.Security.Tests;

public class PasswordPolicyTests
{
    [Theory]
    [InlineData("0000")]
    [InlineData("1234")]
    [InlineData("1111")]
    [InlineData("12ab")]
    [InlineData("123")]
    public void IsPinAllowed_BlocksInvalidPins(string pin)
    {
        Assert.False(PasswordPolicy.IsPinAllowed(pin));
    }

    [Theory]
    [InlineData("2468")]
    [InlineData("9073")]
    public void IsPinAllowed_AllowsValidPins(string pin)
    {
        Assert.True(PasswordPolicy.IsPinAllowed(pin));
    }

    [Fact]
    public void IsPasswordValid_EnforcesComplexity()
    {
        Assert.False(PasswordPolicy.IsPasswordValid("short", out _));
        Assert.False(PasswordPolicy.IsPasswordValid("alllowercase1!", out _));
        Assert.False(PasswordPolicy.IsPasswordValid("NoDigits!!", out _));
        Assert.False(PasswordPolicy.IsPasswordValid("NoSpecial1", out _));
        Assert.True(PasswordPolicy.IsPasswordValid("Admin@1234", out var error));
        Assert.Empty(error);
    }
}
