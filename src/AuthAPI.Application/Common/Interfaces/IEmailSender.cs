using AuthAPI.Domain.Common.Results;

namespace AuthAPI.Application.Common.Interfaces;

public interface IEmailSender
{
    Task<Result> SendAsync(string email, string body);
}