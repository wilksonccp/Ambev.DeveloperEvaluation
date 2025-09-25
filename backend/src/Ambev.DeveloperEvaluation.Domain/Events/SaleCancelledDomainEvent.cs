using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public class SaleCancelledDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public Guid SaleId { get; }
    public string Number { get; }

    public SaleCancelledDomainEvent(Guid saleId, string number)
    {
        SaleId = saleId;
        Number = number;
    }
}
