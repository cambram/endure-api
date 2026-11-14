using Endure.Domain.Modules.Users.Entities;

namespace Endure.Domain.Modules.Users.Repositories;

public interface IAppUserRepository
{
    Task<AppUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task AddAsync(AppUser appUser, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
