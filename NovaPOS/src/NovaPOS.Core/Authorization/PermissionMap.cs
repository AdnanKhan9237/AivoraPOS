using NovaPOS.Core.Enums;
using NovaPOS.Core.Extensions;

namespace NovaPOS.Core.Authorization;

public static class PermissionMap
{
    private static readonly IReadOnlyDictionary<UserRole, HashSet<Permission>> RolePermissions =
        new Dictionary<UserRole, HashSet<Permission>>
        {
            [UserRole.Cashier] =
            [
                Permission.ProcessSale
            ],
            [UserRole.Manager] =
            [
                Permission.ProcessSale,
                Permission.ApplyDiscount,
                Permission.ProcessRefund,
                Permission.ViewReports,
                Permission.ManageProducts,
                Permission.ExportData,
                Permission.VoidSale
            ],
            [UserRole.Admin] = Enum.GetValues<Permission>().ToHashSet()
        };

    public static bool RoleHasPermission(UserRole role, Permission permission) =>
        RolePermissions.TryGetValue(role, out var permissions) && permissions.Contains(permission);
}
