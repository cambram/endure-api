using Endure.Domain.Common.Entities;
using Endure.Domain.Modules.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace Endure.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> AppUsers
    {
        get
        {
            return Set<AppUser>();
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = now;
                entry.Entity.UpdatedAtUtc = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
