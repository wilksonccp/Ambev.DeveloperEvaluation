namespace Ambev.DeveloperEvaluation.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}