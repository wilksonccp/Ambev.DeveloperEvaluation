using Ambev.DeveloperEvaluation.Domain.Exceptions;
using NodaMoney;

namespace Ambev.DeveloperEvaluation.Domain.Policies;

public static class DiscountPolicy
{
    public const int MaxPerItem = 20;

    /// <summary>
    /// Calculates the discount percentage based on the quantity of items.
    /// </summary>
    public static decimal GetRate(int quantity)
    {
        if (quantity < 1)
            throw new DomainException("QUANTITY_MUST_BE_POSITIVE", "Quantity must be at least 1.");

        if (quantity <= 3) return 0m;
        if (quantity <= 9) return 0.1m;
        if (quantity <= MaxPerItem) return 0.2m;

        throw new DomainException("MAX_PER_ITEM_EXCEEDED", $"Quantity per product cannot exceed {MaxPerItem}.");
    }

    public static (decimal discountAmount, decimal lineTotal) CalculateDiscount(decimal initPrice, int quantity)
    {
        if (initPrice <= 0)
            throw new DomainException("UNIT_PRICE_MUST_BE_POSITIVE", "Unit price must be greater than 0.");

        var rate = GetRate(quantity);
        var currency = Currency.FromCode("BRL");
        var unitPrice = new Money(initPrice, currency);
        var gross = unitPrice * quantity; // Money
        var discount = gross * rate;      // Money
        var lineTotal = gross - discount; // Money
        return (discount.Amount, lineTotal.Amount);
    }
}
