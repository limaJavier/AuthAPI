namespace AuthAPI.Api.Tests.Features.Utils.Constants;

public static partial class Constants
{
    public static class User
    {
        public const string Name = "John";
        public const string Email = "john@mail.com";
        public const string Password = "Password0;";
        
        public static readonly List<string> BadPasswords = [
            "",
            "short1!",
            "alllowercase1!",
            "ALLUPPERCASE1!",
            "NoDigits!",
            "NoSpecialChar1",
            new string('a', 256) + "A1!"
        ];

        public static readonly List<string> BadEmails = [
            "",
            "missingatsign.com",
            "missingdomain@",
            "@missingusername.com",
            "toolong" + new string('a', 250) + "@example.com",
        ];
    }
}