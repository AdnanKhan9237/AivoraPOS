using AivoraPOS.Core.Entities;

namespace AivoraPOS.Core.Interfaces.Security;

public interface ICurrentUserService
{
    User? CurrentUser { get; }
    bool IsAuthenticated { get; }
    void SetCurrentUser(User user);
    void Clear();
    event EventHandler? CurrentUserChanged;
}
