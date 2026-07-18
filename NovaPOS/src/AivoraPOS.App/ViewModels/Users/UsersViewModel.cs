using CommunityToolkit.Mvvm.ComponentModel;
using AivoraPOS.Core.Attributes;
using AivoraPOS.Core.Enums;

namespace AivoraPOS.App.ViewModels.Users;

[RequiresPermission(Permission.ManageUsers)]
public partial class UsersViewModel : ObservableObject
{
    [ObservableProperty]
    private string _statusMessage = "User management is available on the Admin plan. Create, edit, and deactivate staff accounts from this screen in a future update.";
}
