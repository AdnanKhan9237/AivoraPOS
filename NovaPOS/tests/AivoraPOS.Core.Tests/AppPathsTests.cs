using AivoraPOS.Core.Constants;

namespace AivoraPOS.Core.Tests;

public class AppPathsTests
{
    [Fact]
    public void DatabaseFilePath_UsesAppDataAivoraPOSDataFolder()
    {
        var expectedSuffix = Path.Combine("AivoraPOS", "data", "aivorapos.db");
        Assert.EndsWith(expectedSuffix.Replace('/', Path.DirectorySeparatorChar), AppPaths.DatabaseFilePath);
    }
}
