using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Infrastructure.Middlewares;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace AuthAPI.Infrastructure.Services;

public class ApplicationEventQueue(
    IHttpContextAccessor httpContextAccessor,
    IPublisher publisher    
) : IApplicationEventQueue
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IPublisher _publisher = publisher;

    public async Task PushAsync(IApplicationEvent applicationEvent)
    {
        if(IsUserWaitingOnline())
        {
            AddToOfflineProcessingQueue(applicationEvent);
        }
        else
        {
            await _publisher.Publish(applicationEvent);
        }
    }

    private bool IsUserWaitingOnline() => _httpContextAccessor.HttpContext is not null;

    private void AddToOfflineProcessingQueue(IApplicationEvent applicationEvent)
    {
        Queue<IApplicationEvent> applicationEventsQueue = _httpContextAccessor.HttpContext!.Items.TryGetValue(EventualConsistencyMiddleware.ApplicationEventsKey, out var value) &&
            value is Queue<IApplicationEvent> existingApplicationEvents
            ? existingApplicationEvents
            : new();

        applicationEventsQueue.Enqueue(applicationEvent);
        _httpContextAccessor.HttpContext.Items[EventualConsistencyMiddleware.ApplicationEventsKey] = applicationEventsQueue;
    }
}
