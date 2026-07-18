namespace NovaPOS.Core.Constants;

public static class AppPaths
{
    public const string AppFolderName = "NovaPOS";

    public static string AppDataRoot =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppFolderName);

    public static string DataDirectory => Path.Combine(AppDataRoot, "data");

    public static string LogsDirectory => Path.Combine(AppDataRoot, "logs");

    public static string DatabaseFilePath => Path.Combine(DataDirectory, "novapos.db");

    public static string ReceiptsDirectory => Path.Combine(AppDataRoot, "receipts");

    public static string ReportsDirectory => Path.Combine(AppDataRoot, "reports");

    public static void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(DataDirectory);
        Directory.CreateDirectory(LogsDirectory);
        Directory.CreateDirectory(ReceiptsDirectory);
        Directory.CreateDirectory(ReportsDirectory);
    }
}
