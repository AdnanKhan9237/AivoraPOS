using CommunityToolkit.Mvvm.ComponentModel;

namespace NovaPOS.App.ViewModels.Settings;

public partial class UserListItemVm : ObservableObject
{
    public UserListItemVm(Guid id, string fullName, string role, bool isActive, DateTime? lastLoginAt)
    {
        Id = id;
        FullName = fullName;
        Role = role;
        IsActive = isActive;
        LastLoginAt = lastLoginAt;
        StatusText = isActive ? "Active" : "Inactive";
        LastLoginText = lastLoginAt?.ToLocalTime().ToString("g") ?? "Never";
    }

    public Guid Id { get; }
    public string FullName { get; }
    public string Role { get; }
    public bool IsActive { get; }
    public string StatusText { get; }
    public string LastLoginText { get; }
    public DateTime? LastLoginAt { get; }
}
