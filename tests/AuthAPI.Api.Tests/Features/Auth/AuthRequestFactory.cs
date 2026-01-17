using AuthAPI.Api.Features.Auth.AddPassword;
using AuthAPI.Api.Features.Auth.ChangePassword;
using AuthAPI.Api.Features.Auth.ForgotPassword;
using AuthAPI.Api.Features.Auth.LoginWithEmail;
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

    public static LoginWithEmailRequest CreateLoginRequest(
        string email = Constants.User.Email,
        string password = Constants.User.Password
    ) => new
    (
        Email: email,
        Password: password
    );

    public static ForgotPasswordRequest CreateForgotPasswordRequest(
        string email = Constants.User.Email
    ) => new
    (
        Email: email
    );

    public static ChangePasswordRequest CreateChangePasswordRequest(
        string oldPassword = Constants.User.Password,
        string newPassword = "New" + Constants.User.Password
    ) => new
    (
        OldPassword: oldPassword,
        NewPassword: newPassword
    );

    public static AddPasswordRequest CreateAddPasswordRequest(
        string password = Constants.User.Password
    ) => new
    (
        Password: password
    );
}
