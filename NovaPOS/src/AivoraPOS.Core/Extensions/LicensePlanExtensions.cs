using AivoraPOS.Core.Enums;

namespace AivoraPOS.Core.Extensions;

public static class LicensePlanExtensions
{
    public static int? GetMaxCashiers(this LicensePlan plan) => plan switch
    {
        LicensePlan.Starter => 2,
        LicensePlan.Professional => 10,
        LicensePlan.Enterprise => null,
        _ => 2
    };

    public static int? GetMaxProducts(this LicensePlan plan) => plan switch
    {
        LicensePlan.Starter => 500,
        LicensePlan.Professional => 5_000,
        LicensePlan.Enterprise => null,
        _ => 500
    };

    public static bool Supports(this LicensePlan plan, LicenseFeature feature) => feature switch
    {
        LicenseFeature.FullReports => plan is LicensePlan.Professional or LicensePlan.Enterprise,
        LicenseFeature.ReceiptCustomization => plan is LicensePlan.Professional or LicensePlan.Enterprise,
        LicenseFeature.ExportPdfExcel => plan is LicensePlan.Professional or LicensePlan.Enterprise,
        LicenseFeature.MultiCurrency => plan is LicensePlan.Enterprise,
        LicenseFeature.PrioritySupport => plan is LicensePlan.Enterprise,
        _ => false
    };

    public static bool UsesBasicReportsOnly(this LicensePlan plan) =>
        plan is LicensePlan.Starter;
}
