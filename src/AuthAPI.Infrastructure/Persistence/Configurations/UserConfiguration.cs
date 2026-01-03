using AuthAPI.Domain.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthAPI.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);

        builder.Property(user => user.Name)
            .HasMaxLength(200);

        builder.Property(user => user.Email)
            .HasMaxLength(200);

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(500);

        // Email must be unique for active users
        builder.HasIndex(user => new { user.Email, user.IsActive })
            .IsUnique()
            .HasFilter("\"IsActive\" = true");
    }
}
