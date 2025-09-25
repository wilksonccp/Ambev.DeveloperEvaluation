using System.Text.Json;
using Ambev.DeveloperEvaluation.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.IoC.DomainEvents;

public class LoggingDomainEventPublisher : IDomainEventPublisher
{
    private readonly ILogger<LoggingDomainEventPublisher> _logger;

    public LoggingDomainEventPublisher(ILogger<LoggingDomainEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(IDomainEvent @event, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(@event);
        _logger.LogInformation("DomainEvent published: {EventType} {Payload}", @event.GetType().Name, payload);
        return Task.CompletedTask;
    }
}

