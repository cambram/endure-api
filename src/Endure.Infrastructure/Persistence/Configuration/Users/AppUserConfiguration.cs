using Endure.Domain.Modules.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Endure.Infrastructure.Persistence.Configuration.Users;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("AppUsers", ConfigureTable);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);
    }

    private static void ConfigureTable(TableBuilder<AppUser> table)
    {
        table.HasCheckConstraint("CK_AppUsers_Email_Lowercase", "\"Email\" = lower(\"Email\")");
    }
}
