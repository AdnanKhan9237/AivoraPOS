using NovaPOS.Core.Constants;

namespace NovaPOS.Core.Tests;

public class AppPathsTests
{
    [Fact]
    public void DatabaseFilePath_UsesAppDataNovaPOSDataFolder()
    {
        var expectedSuffix = Path.Combine("NovaPOS", "data", "novapos.db");
        Assert.EndsWith(expectedSuffix.Replace('/', Path.DirectorySeparatorChar), AppPaths.DatabaseFilePath);
    }
}
