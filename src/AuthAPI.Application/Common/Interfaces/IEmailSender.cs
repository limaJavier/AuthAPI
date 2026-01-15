using AuthAPI.Domain.Common.Results;

namespace AuthAPI.Application.Common.Interfaces;

public interface IEmailSender
{
    Task<Result> SendAsync(string toEmail, string subject, string body);
}
