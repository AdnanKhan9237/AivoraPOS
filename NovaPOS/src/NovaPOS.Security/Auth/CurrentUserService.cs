using NovaPOS.Core.Entities;
using NovaPOS.Core.Interfaces.Security;

namespace NovaPOS.Security.Auth;

public sealed class CurrentUserService : ICurrentUserService
{
    public User? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser is not null;

    public event EventHandler? CurrentUserChanged;

    public void SetCurrentUser(User user)
    {
        CurrentUser = user;
        CurrentUserChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        CurrentUser = null;
        CurrentUserChanged?.Invoke(this, EventArgs.Empty);
    }
}
