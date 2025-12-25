namespace AuthAPI.Application.Common.Interfaces;

public interface IApplicationEventQueue
{
    Task PushAsync(IApplicationEvent applicationEvent);
}
