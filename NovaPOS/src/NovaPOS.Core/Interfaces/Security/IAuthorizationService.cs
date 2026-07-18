using NovaPOS.Core.Enums;

namespace NovaPOS.Core.Interfaces.Security;

public interface IAuthorizationService
{
    bool HasPermission(Permission permission);
    void RequirePermission(Permission permission);
}
