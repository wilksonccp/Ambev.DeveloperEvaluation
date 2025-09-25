using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public class SaleModifiedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public Guid SaleId { get; }
    public string Change { get; }
    public Guid? ProductId { get; }
    public int? Quantity { get; }
    public decimal TotalAmount { get; }
    public decimal TotalDiscount { get; }
    public decimal TotalPayable { get; }

    public SaleModifiedDomainEvent(
        Guid saleId,
        string change,
        decimal totalAmount,
        decimal totalDiscount,
        decimal totalPayable,
        Guid? productId = null,
        int? quantity = null)
    {
        SaleId = saleId;
        Change = change;
        ProductId = productId;
        Quantity = quantity;
        TotalAmount = totalAmount;
        TotalDiscount = totalDiscount;
        TotalPayable = totalPayable;
    }
}
