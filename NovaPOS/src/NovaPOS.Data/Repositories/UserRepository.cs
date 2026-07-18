using Microsoft.EntityFrameworkCore;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Interfaces.Repositories;

namespace NovaPOS.Data.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Username == username, cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(x => x.Username == username, cancellationToken);
    }
}
