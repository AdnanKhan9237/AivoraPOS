using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaPOS.Core.Attributes;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Interfaces.Repositories;

namespace NovaPOS.App.ViewModels;

[RequiresPermission(Permission.ViewAuditLog)]
public partial class AuditLogViewModel : ObservableObject
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUserRepository _userRepository;

    public AuditLogViewModel(IAuditLogRepository auditLogRepository, IUserRepository userRepository)
    {
        _auditLogRepository = auditLogRepository;
        _userRepository = userRepository;
        EndDate = DateTime.Today;
        StartDate = DateTime.Today.AddDays(-7);
        _ = InitializeAsync();
    }

    public ObservableCollection<AuditLog> Entries { get; } = new();
    public ObservableCollection<User> Users { get; } = new();

    [ObservableProperty]
    private DateTime _startDate;

    [ObservableProperty]
    private DateTime _endDate;

    [ObservableProperty]
    private string _actionFilter = string.Empty;

    [ObservableProperty]
    private User? _selectedUser;

    [RelayCommand]
    private async Task SearchAsync()
    {
        var results = await _auditLogRepository.SearchAsync(
            StartDate.ToUniversalTime(),
            EndDate.AddDays(1).ToUniversalTime(),
            SelectedUser?.Id,
            string.IsNullOrWhiteSpace(ActionFilter) ? null : ActionFilter.Trim());

        Entries.Clear();
        foreach (var entry in results)
        {
            Entries.Add(entry);
        }
    }

    private async Task InitializeAsync()
    {
        var users = await _userRepository.GetActiveUsersAsync();
        Users.Clear();
        foreach (var user in users)
        {
            Users.Add(user);
        }

        await SearchAsync();
    }
}
