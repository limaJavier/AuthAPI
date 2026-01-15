using AuthAPI.Domain.SessionAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthAPI.Infrastructure.Persistence.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(user => user.Id);

        builder.HasMany(session => session.RefreshTokens)
            .WithOne(token => token.Session)
            .HasForeignKey(token => token.SessionId);
    }
}
