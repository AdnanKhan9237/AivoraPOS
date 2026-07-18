using CommunityToolkit.Mvvm.ComponentModel;
using NovaPOS.Core.Attributes;
using NovaPOS.Core.Enums;

namespace NovaPOS.App.ViewModels.Users;

[RequiresPermission(Permission.ManageUsers)]
public partial class UsersViewModel : ObservableObject
{
    [ObservableProperty]
    private string _statusMessage = "User management is available on the Admin plan. Create, edit, and deactivate staff accounts from this screen in a future update.";
}
