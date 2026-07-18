using System.Windows;
using NovaPOS.Core.Attributes;
using NovaPOS.Core.Interfaces.Security;

namespace NovaPOS.App.Behaviors;

public static class AuthorizationBehavior
{
    public static bool CanNavigateTo(object viewModel, IAuthorizationService authorizationService)
    {
        var permission = viewModel.GetType().GetCustomAttributes(typeof(RequiresPermissionAttribute), true)
            .Cast<RequiresPermissionAttribute>()
            .FirstOrDefault()?.Permission;

        if (permission is null)
        {
            return true;
        }

        if (!authorizationService.HasPermission(permission.Value))
        {
            MessageBox.Show(
                "You do not have permission to access this screen.",
                "Access Denied",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        return true;
    }
}
