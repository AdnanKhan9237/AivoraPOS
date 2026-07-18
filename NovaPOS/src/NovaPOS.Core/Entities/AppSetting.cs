namespace NovaPOS.Core.Entities;

public class AppSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string? Description { get; set; }
    public int? UpdatedByUserId { get; set; }

    public User? UpdatedByUser { get; set; }
}
