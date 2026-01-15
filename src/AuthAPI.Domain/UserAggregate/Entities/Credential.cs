using AuthAPI.Domain.Common;
using AuthAPI.Domain.UserAggregate.Enums;

namespace AuthAPI.Domain.UserAggregate.Entities;

public class Credential : Entity
{
    public string Identifier { get; set; } = null!;
    public CredentialType Type { get; set; }
}
