using NovaPOS.Core.Enums;

namespace NovaPOS.Core.Models.Settings;

public sealed class LicenseDetailsDto
{
    public string PlanName { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public string MachineId { get; set; } = string.Empty;
    public string? LicensedBusinessName { get; set; }
    public bool IsTrial { get; set; }
    public int? TrialDaysRemaining { get; set; }
}
