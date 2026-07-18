using NovaPOS.Core.Authorization;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Exceptions;
using NovaPOS.Core.Interfaces.Security;

namespace NovaPOS.Security.Auth;

public sealed class AuthorizationService : IAuthorizationService
{
    private readonly ICurrentUserService _currentUserService;

    public AuthorizationService(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public bool HasPermission(Permission permission)
    {
        var user = _currentUserService.CurrentUser;
        return user is not null && PermissionMap.RoleHasPermission(user.Role, permission);
    }

    public void RequirePermission(Permission permission)
    {
        if (!HasPermission(permission))
        {
            throw new AuthorizationException($"You do not have permission to perform this action ({permission}).");
        }
    }
}
