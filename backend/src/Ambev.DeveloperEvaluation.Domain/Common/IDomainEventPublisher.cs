namespace Ambev.DeveloperEvaluation.Domain.Common;

public interface IDomainEventPublisher
{
    Task PublishAsync(IDomainEvent @event, CancellationToken cancellationToken = default);
}

