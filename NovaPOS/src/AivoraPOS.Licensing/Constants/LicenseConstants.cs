namespace AivoraPOS.Licensing.Constants;

public static class LicenseConstants
{
    public const string KeyPrefix = "NOVA";
    public const int GracePeriodDays = 7;
    public const int TrialDays = 30;
    public const int TrialMaxProducts = 50;
    public const int MaxComponentMismatches = 1;
    public const string CacheFileName = "license.cache";

    public static readonly byte[] DefaultVerificationKey =
        "AivoraPOS-License-VerifyKey-v1-32b!"u8.ToArray();
}
