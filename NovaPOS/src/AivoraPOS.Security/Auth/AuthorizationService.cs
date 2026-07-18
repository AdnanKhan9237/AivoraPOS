using AivoraPOS.Core.Authorization;
using AivoraPOS.Core.Enums;
using AivoraPOS.Core.Exceptions;
using AivoraPOS.Core.Interfaces.Security;

namespace AivoraPOS.Security.Auth;

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
