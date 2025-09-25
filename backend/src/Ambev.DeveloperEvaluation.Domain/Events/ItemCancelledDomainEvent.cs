using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public class ItemCancelledDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public Guid SaleId { get; }
    public IReadOnlyCollection<Guid> ProductIds { get; }

    public ItemCancelledDomainEvent(Guid saleId, IEnumerable<Guid> productIds)
    {
        SaleId = saleId;
        ProductIds = productIds.ToArray();
    }
}
