using AuthAPI.Domain.UserAggregate.Enums;
using AuthAPI.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthAPI.Infrastructure.Persistence.Configurations;

public class CredentialConfiguration : IEntityTypeConfiguration<Credential>
{
    public void Configure(EntityTypeBuilder<Credential> builder)
    {
        builder.HasKey(credential => new { credential.Identifier, credential.Type, credential.UserId });

        builder.Property(credential => credential.Identifier)
            .HasMaxLength(200);

        builder.Property(credential => credential.Type)
            .HasConversion(
                pIn => pIn.ToString(),
                pOut => Enum.Parse<CredentialType>(pOut)
            );

        builder.HasOne(credential => credential.User)
            .WithMany(user => user.Credentials)
            .HasForeignKey(credential => credential.UserId);

        builder.HasIndex(credential => new { credential.Type, credential.UserId });
    }
}
