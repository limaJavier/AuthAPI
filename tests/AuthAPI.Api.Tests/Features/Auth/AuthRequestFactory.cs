using AuthAPI.Api.Features.Auth.RegisterWithEmail;
using AuthAPI.Api.Tests.Features.Utils.Constants;

namespace AuthAPI.Api.Tests.Features.Auth;

public static class AuthRequestsFactory
{
    public static RegisterWithEmailRequest CreateRegisterRequest(
        string name = Constants.User.Name,
        string email = Constants.User.Email,
        string password = Constants.User.Password
    ) => new
    (
        Name: name,
        Email: email,
        Password: password
    );
}
