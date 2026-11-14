using Endure.Application.Common.Exceptions;
using Endure.Domain.Modules.Users.Entities;
using Endure.Domain.Modules.Users.Repositories;
using Endure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Endure.Infrastructure.Repositories.Users;

internal sealed class AppUserRepository : IAppUserRepository
{
    private readonly ApplicationDbContext _context;

    public AppUserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<AppUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _context.AppUsers.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return _context.AppUsers.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task AddAsync(AppUser appUser, CancellationToken cancellationToken)
    {
        await _context.AppUsers.AddAsync(appUser, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConflictException("An account with this email already exists.");
        }
    }
}
