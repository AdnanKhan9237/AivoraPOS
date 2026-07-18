using AivoraPOS.Core.Enums;

namespace AivoraPOS.Core.Interfaces.Security;

public interface IAuthorizationService
{
    bool HasPermission(Permission permission);
    void RequirePermission(Permission permission);
}
