using NovaPOS.Core.Entities;

namespace NovaPOS.Core.Interfaces.Security;

public interface ICurrentUserService
{
    User? CurrentUser { get; }
    bool IsAuthenticated { get; }
    void SetCurrentUser(User user);
    void Clear();
    event EventHandler? CurrentUserChanged;
}
