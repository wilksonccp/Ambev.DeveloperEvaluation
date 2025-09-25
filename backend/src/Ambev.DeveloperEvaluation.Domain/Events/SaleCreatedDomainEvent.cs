using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public class SaleCreatedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public Guid SaleId { get; }
    public string Number { get; }
    public Guid CustomerId { get; }
    public Guid BranchId { get; }
    public decimal TotalAmount { get; }
    public decimal TotalDiscount { get; }
    public decimal TotalPayable { get; }

    public SaleCreatedDomainEvent(
        Guid saleId,
        string number,
        Guid customerId,
        Guid branchId,
        decimal totalAmount,
        decimal totalDiscount,
        decimal totalPayable)
    {
        SaleId = saleId;
        Number = number;
        CustomerId = customerId;
        BranchId = branchId;
        TotalAmount = totalAmount;
        TotalDiscount = totalDiscount;
        TotalPayable = totalPayable;
    }
}
