using AuthAPI.Domain.Common;
using AuthAPI.Domain.UserAggregate.Enums;

namespace AuthAPI.Domain.UserAggregate.ValueObjects;

public class Credential : ValueObject
{
    public string Identifier { get; private set; } = null!;
    public CredentialType Type { get; private set; }

    // Relational properties
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Identifier;
        yield return Type;
    }

    private Credential() { }

    public static Credential Create(string identifier, CredentialType type) => new()
    {
        Identifier = identifier,
        Type = type
    };
}
