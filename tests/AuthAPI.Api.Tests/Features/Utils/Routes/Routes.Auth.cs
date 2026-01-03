namespace AuthAPI.Api.Tests.Features.Utils.Routes;

public static partial class Routes
{
    public static class Auth
    {
        public const string Register = "/auth/register";
        public const string VerifyEmail = "/auth/verify-email";
        public const string Login = "/auth/login";
        public const string Refresh = "/auth/refresh";
        public const string Logout = "/auth/logout";
        public const string ForgotPassword = "/auth/forgot-password";
    }
}