using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Policies;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleItem
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;

    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public decimal DiscountAmount { get; private set; }
    public decimal LineTotal { get; private set; }

    public bool IsCancelled { get; private set; } = false;

    protected SaleItem() { } // For EF

    internal SaleItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        // Validations
        if (productId == Guid.Empty)
            throw new DomainException("INVALID_PRODUCT_ID", "Product ID must be a valid GUID.");
        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("INVALID_PRODUCT_NAME", "Product name cannot be empty.");
        if (unitPrice <= 0)
            throw new DomainException("UNIT_PRICE_MUST_BE_POSITIVE", "Unit price must be greater than 0.");
        if (quantity < 1)
            throw new DomainException("QUANTITY_MUST_BE_POSITIVE", "Quantity must be at least 1.");
        if (quantity > DiscountPolicy.MaxPerItem)
            throw new DomainException("MAX_PER_ITEM_EXCEEDED", $"Quantity per product cannot exceed {DiscountPolicy.MaxPerItem}.");

        // Initialization
        ProductId = productId;
        ProductName = productName.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;

        Recalculate();
    }

    internal void SetQuantity(int newQuantity)
    {
        if (IsCancelled)
            throw new DomainException("ITEM_CANCELLED", "Cannot change quantity of a cancelled item.");
        if (newQuantity < 1)
            throw new DomainException("QUANTITY_MUST_BE_POSITIVE", "Quantity must be at least 1.");

        // DiscountPolicy will handle max per item validation
        Quantity = newQuantity;
    }

    internal void IncreaseQuantity(int increment)
    {
        if (IsCancelled)
            throw new DomainException("ITEM_CANCELLED", "Cannot change quantity of a cancelled item.");
        if (increment < 1)
            throw new DomainException("INCREMENT_MUST_BE_POSITIVE", "Increment must be at least 1.");

        // DiscountPolicy will handle max per item validation
        Quantity += increment;
    }
    internal void EnsureSameUnitPrice(decimal unitPrice)
    {
        if (unitPrice <= 0)
            throw new DomainException("UNIT_PRICE_MUST_BE_POSITIVE", "Unit price must be greater than 0.");
        if (UnitPrice != unitPrice)
            throw new DomainException("UNIT_PRICE_MISMATCH", "Unit price does not match existing item price.");
    }

    internal void Cancel()
    {
        if (IsCancelled)
            throw new DomainException("ITEM_ALREADY_CANCELLED", "Item is already cancelled.");

        IsCancelled = true;
        Quantity = 0;
        DiscountAmount = 0;
        LineTotal = 0;
    }

    internal void Recalculate()
    {
        if (IsCancelled)
            throw new DomainException("ITEM_CANCELLED", "Cannot recalculate a cancelled item.");

        var (discountAmount, lineTotal) = DiscountPolicy.CalculateDiscount(UnitPrice, Quantity);
        DiscountAmount = discountAmount;
        LineTotal = lineTotal;
    }
}
