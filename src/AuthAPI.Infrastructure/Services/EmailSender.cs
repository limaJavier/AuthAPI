using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Domain.Common.Results;

namespace AuthAPI.Infrastructure.Services;

public class EmailSender : IEmailSender
{
    public async Task<Result> SendAsync(string email, string body)
    {
        await Task.CompletedTask;
        Console.WriteLine($"Sending email to {email}: {body}");
        return Result.Success();
    }
}