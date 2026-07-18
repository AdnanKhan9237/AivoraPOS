using AivoraPOS.Core.Authorization;
using AivoraPOS.Core.Enums;

namespace AivoraPOS.Security.Tests;

public class PermissionMapTests
{
    [Theory]
    [InlineData(UserRole.Cashier, Permission.ProcessSale, true)]
    [InlineData(UserRole.Cashier, Permission.ApplyDiscount, false)]
    [InlineData(UserRole.Manager, Permission.VoidSale, true)]
    [InlineData(UserRole.Manager, Permission.ManageUsers, false)]
    [InlineData(UserRole.Admin, Permission.ViewAuditLog, true)]
    [InlineData(UserRole.Admin, Permission.ChangePrices, true)]
    public void RoleHasPermission_MatchesMatrix(UserRole role, Permission permission, bool expected)
    {
        Assert.Equal(expected, PermissionMap.RoleHasPermission(role, permission));
    }
}
