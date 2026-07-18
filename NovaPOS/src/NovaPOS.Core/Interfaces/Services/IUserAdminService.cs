using NovaPOS.Core.Entities;
using NovaPOS.Core.Enums;

namespace NovaPOS.Core.Interfaces.Services;

public interface IUserAdminService
{
    Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<User> CreateUserAsync(string fullName, string username, UserRole role, string pin, string password, CancellationToken cancellationToken = default);
    Task<User> UpdateUserAsync(Guid userId, string fullName, UserRole role, bool isActive, CancellationToken cancellationToken = default);
    Task ResetPinAsync(Guid userId, string newPin, CancellationToken cancellationToken = default);
    Task DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task ChangeOwnPasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
}
