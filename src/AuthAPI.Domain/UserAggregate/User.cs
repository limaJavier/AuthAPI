using AuthAPI.Domain.Common;
using AuthAPI.Domain.Common.Interfaces;
using AuthAPI.Domain.Common.Results;
using AuthAPI.Domain.SessionAggregate;
using AuthAPI.Domain.UserAggregate.Enums;
using AuthAPI.Domain.UserAggregate.ValueObjects;

namespace AuthAPI.Domain.UserAggregate;

public class User : AggregateRoot
{
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? PasswordHash { get; private set; }
    public bool IsVerified { get; private set; } = false;
    public bool IsActive { get; private set; } = true;

    // Relational properties
    public ICollection<Credential> Credentials { get; private set; } = null!; // Belongs to user aggregate
    public ICollection<Session> Sessions { get; private set; } = null!;

    private User() { }

    public static User Create(
        string name,
        string email,
        string password,
        IHasher hasher
    )
    {
        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = hasher.Hash(password)
        };

        return user;
    }

    public static User Create(
        string name,
        string email,
        string credentialIdentifier,
        CredentialType credentialType
    )
    {
        var user = new User
        {
            Name = name,
            Email = email,
            IsVerified = true,
            Credentials = [Credential.Create(credentialIdentifier, credentialType)]
        };

        return user;
    }

    public Result VerifyPassword(string password, IHasher hasher)
    {
        if (PasswordHash is null)
            return Error.Conflict("User does not have a password");

        if (hasher.Verify(password, PasswordHash))
            return Result.Success();
        else
            return Error.Conflict("Wrong password");
    }

    public Result ChangePassword(string password, IHasher hasher)
    {
        if (!IsVerified)
            return Error.Conflict($"User with ID {Id} is not verified");

        if (PasswordHash is null)
            return Error.Conflict("User does not have a password");

        PasswordHash = hasher.Hash(password);
        return Result.Success();
    }

    public Result Verify()
    {
        if (IsVerified)
            return Error.Conflict($"User with ID {Id} is already verified");
        IsVerified = true;
        return Result.Success();
    }

    public Result AddPassword(string password, IHasher hasher)
    {
        if (!IsVerified)
            return Error.Conflict($"User with ID {Id} is not verified");

        if (PasswordHash is not null)
            return Error.Conflict($"User with ID {Id} already has a password");

        PasswordHash = hasher.Hash(password);
        return Result.Success();
    }

    public Result AddCredential(string credentialIdentifier, CredentialType credentialType)
    {
        if (!IsVerified)
            return Error.Conflict($"User with ID {Id} is not verified");

        if (Credentials.Any(credential => credential.Type == credentialType))
            return Error.Conflict($"User with ID {Id} already have a credential of type {credentialType}");

        Credentials.Add(Credential.Create(credentialIdentifier, credentialType));
        return Result.Success();
    }
}
