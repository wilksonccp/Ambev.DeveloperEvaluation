using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Policies;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.SaleTests;

public class SaleItem_Tests
{
    // Creation and initialization

    [Fact(DisplayName = "Creation: must trim name and initialize fields")]
    public void Create_ShouldTrimName_AndInitialize()
    {
        // Arrange
        var sale = Sale.CreateNew(
            id: Guid.NewGuid(),
            number: " S-0001 ",
            customerId: Guid.NewGuid(),
            customerName: " Test Client ",
            branchId: Guid.NewGuid(),
            branchName: " Branch Center ");

        var productId = Guid.NewGuid();

        // Act
        sale.AddItem(productId, "  Lager beer  ", 10m, 1);

        // Assert
        sale.ReadOnlyItems.Should().HaveCount(1);
        var item = sale.ReadOnlyItems.Single();

        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be("Lager beer"); // trimmed
        item.Quantity.Should().Be(1);
        item.UnitPrice.Should().Be(10m);
        item.IsCancelled.Should().BeFalse();

        // Range 1-3: No discount
        item.DiscountAmount.Should().Be(0m);
        item.LineTotal.Should().Be(10m);

        // Sales totals reflecting the item
        sale.TotalAmount.Should().Be(10m);
        sale.TotalDiscount.Should().Be(0m);
        sale.TotalPayable.Should().Be(10m);
        sale.IsCancelled.Should().BeFalse();
    }

    [Fact(DisplayName = "Insert: empty ProductId must fail")]
    public void Create_WithEmptyProductId_ShouldThrow()
    {
        // Arrange
        var sale = Sale.CreateNew(
            id: Guid.NewGuid(),
            number: "S-0001",
            customerId: Guid.NewGuid(),
            customerName: "Test Client",
            branchId: Guid.NewGuid(),
            branchName: "Branch Center");

        var productId = Guid.Empty;

        // Act
        Action act = () => sale.AddItem(productId, "Lager beer", 10m, 1);

        // Assert
        var ex = act.Should().Throw<DomainException>().Which;
        ex.ErrorCode.Should().Be("INVALID_PRODUCT_ID");
        ex.Message.Should().Be("Product ID must be a valid GUID.");
        
    }

    [Theory(DisplayName = "Invalid creation: UnitPrice <= 0 should fail")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidUnitPrice_ShouldThrow(decimal price)
    {
        // Arrange
        var sale = Sale.CreateNew(
            id: Guid.NewGuid(),
            number: "S-0001",
            customerId: Guid.NewGuid(),
            customerName: "Test Client",
            branchId: Guid.NewGuid(),
            branchName: "Branch Center");

        var productId = Guid.NewGuid();

        // Act
        Action act = () => sale.AddItem(productId, "Lager beer", price, 1);

        // Assert
        var ex = act.Should().Throw<DomainException>().Which;
        ex.ErrorCode.Should().Be("UNIT_PRICE_MUST_BE_POSITIVE");
        ex.Message.Should().Be("Unit price must be greater than 0.");
    }

    [Theory(DisplayName = "Invalid creation: Quantity out of range should fail")]
    [InlineData(0, "QUANTITY_MUST_BE_POSITIVE", "Quantity must be at least 1.")]
    [InlineData(DiscountPolicy.MaxPerItem + 1, "MAX_PER_ITEM_EXCEEDED", null)]
    public void Create_WithInvalidQuantity_ShouldThrow(int qty, string expectedCode, string? expectedMessage)
    {
        // Arrange
        var sale = Sale.CreateNew(
            id: Guid.NewGuid(),
            number: "S-0001",
            customerId: Guid.NewGuid(),
            customerName: "Test Client",
            branchId: Guid.NewGuid(),
            branchName: "Branch Center");

        var productId = Guid.NewGuid();

        // Act
        Action act = () => sale.AddItem(productId, "Lager beer", 10m, qty);

        // Assert
        var ex = act.Should().Throw<DomainException>().Which;
        ex.ErrorCode.Should().Be(expectedCode);
        if (expectedMessage is not null)
        {
            ex.Message.Should().Be(expectedMessage);
        }
        else
        {
            ex.Message.Should().Be($"Quantity per product cannot exceed {DiscountPolicy.MaxPerItem}.");
        }
    }

    // Faixas de desconto por quantidade (via AddItem)

    [Theory(DisplayName = "Discount tiers: 1/3=0%, 4/9=10%, 10/20=20%")]
    [InlineData(10.00, 1, 0.00, 10.00)]
    [InlineData(10.00, 3, 0.00, 30.00)]
    [InlineData(10.00, 4, 4.00, 36.00)]
    [InlineData(10.00, 9, 9.00, 81.00)]
    [InlineData(10.00, 10, 20.00, 80.00)]
    [InlineData(10.00, 20, 40.00, 160.00)]
    public void Discount_Tiers_ShouldApply_OnAddItem(double unitPrice, int qty, double expectedDiscount, double expectedLineTotal)
    {
        // Arrange
        var sale = Sale.CreateNew(
            id: Guid.NewGuid(),
            number: "S-0001",
            customerId: Guid.NewGuid(),
            customerName: "Test Client",
            branchId: Guid.NewGuid(),
            branchName: "Branch Center");

        var productId = Guid.NewGuid();

        // Act
        sale.AddItem(productId, "Lager beer", (decimal)unitPrice, qty);

        // Assert
        sale.ReadOnlyItems.Should().HaveCount(1);
        var item = sale.ReadOnlyItems.Single();

        item.Quantity.Should().Be(qty);
        item.UnitPrice.Should().Be((decimal)unitPrice);
        item.DiscountAmount.Should().Be((decimal)expectedDiscount);
        item.LineTotal.Should().Be((decimal)expectedLineTotal);

        // Sales totals reflecting the item
        sale.TotalAmount.Should().Be((decimal)(unitPrice * qty));
        sale.TotalDiscount.Should().Be((decimal)expectedDiscount);
        sale.TotalPayable.Should().Be((decimal)expectedLineTotal);
    }

    [Fact(DisplayName = "Invalid discount: > MaxPerItem should fail on AddItem")]
    public void Discount_OverMax_ShouldThrow_OnAddItem()
    {
        // Arrange
        var sale = Sale.CreateNew(
            id: Guid.NewGuid(),
            number: "S-0001",
            customerId: Guid.NewGuid(),
            customerName: "Test Client",
            branchId: Guid.NewGuid(),
            branchName: "Branch Center");

        var productId = Guid.NewGuid();

        // Act
        Action act = () => sale.AddItem(productId, "Lager beer", 10m, DiscountPolicy.MaxPerItem + 1);

        // Assert
        var ex = act.Should().Throw<DomainException>().Which;
        ex.ErrorCode.Should().Be("MAX_PER_ITEM_EXCEEDED");
        ex.Message.Should().Be($"Quantity per product cannot exceed {DiscountPolicy.MaxPerItem}.");
    }

    // Recalculo ao alterar quantidade

    [Fact(DisplayName = "Increasing quantity crosses ranges and recalculates")]
    public void Increase_CrossTiers_ShouldRecalculate()
    {
        // Arrange
        var sale = Sale.CreateNew(
            id: Guid.NewGuid(),
            number: "S-0001",
            customerId: Guid.NewGuid(),
            customerName: "Test Client",
            branchId: Guid.NewGuid(),
            branchName: "Branch Center");

        var productId = Guid.NewGuid();

        // Act
        sale.AddItem(productId, "Lager beer", 10m, 3);
        sale.AddItem(productId, "Lager beer", 10m, 4);

        // Assert
        sale.ReadOnlyItems.Should().HaveCount(1);
        var item = sale.ReadOnlyItems.Single();

        item.Quantity.Should().Be(7);
        item.UnitPrice.Should().Be(10m);
        item.DiscountAmount.Should().Be(7m);
        item.LineTotal.Should().Be(63m);

        // Sales totals reflecting the item
        sale.TotalAmount.Should().Be(70m);
        sale.TotalDiscount.Should().Be(7m);
        sale.TotalPayable.Should().Be(63m);
    }

    [Fact(DisplayName = "Reduce quantity crosses ranges and recalculate")]
    public void Decrease_CrossTiers_ShouldRecalculate()
    {
        // Arrange
        var sale = Sale.CreateNew(
            id: Guid.NewGuid(),
            number: "S-0001",
            customerId: Guid.NewGuid(),
            customerName: "Test Client",
            branchId: Guid.NewGuid(),
            branchName: "Branch Center");

        var productId = Guid.NewGuid();

        // Act
        sale.AddItem(productId, "Lager beer", 10m, 10);
        sale.RemoveItem(productId, 4);

        // Assert
        sale.ReadOnlyItems.Should().HaveCount(1);
        var item = sale.ReadOnlyItems.Single();

        item.Quantity.Should().Be(6);
        item.UnitPrice.Should().Be(10m);
        item.DiscountAmount.Should().Be(6m);
        item.LineTotal.Should().Be(54m);

        // Sales totals reflecting the item
        sale.TotalAmount.Should().Be(60m);
        sale.TotalDiscount.Should().Be(6m);
        sale.TotalPayable.Should().Be(54m);
    }

    // SetQuantity e limites

    [Fact(DisplayName = "SetQuantity(0) should fail and maintain state")]
    public void SetQuantity_Zero_ShouldThrow_AndKeepState()
    {
        // Arrange
        var sale = Sale.CreateNew(
            id: Guid.NewGuid(),
            number: "S-0001",
            customerId: Guid.NewGuid(),
            customerName: "Test Client",
            branchId: Guid.NewGuid(),
            branchName: "Branch Center");

        var productId = Guid.NewGuid();
        sale.AddItem(productId, "Lager beer", 10m, 3);

        // Act
        Action act = () => sale.UpdateItemQuantity(productId, 0);

        // Assert
        var ex = act.Should().Throw<DomainException>().Which;
        ex.ErrorCode.Should().Be("QUANTITY_MUST_BE_POSITIVE");
        ex.Message.Should().Be("Quantity must be at least 1.");

        // Ensure state is maintained
        sale.ReadOnlyItems.Should().HaveCount(1);
        var item = sale.ReadOnlyItems.Single();
        item.Quantity.Should().Be(3);
    }

    [Fact(DisplayName = "SetQuantity(>Max) should fail and maintain state")]
    public void SetQuantity_OverMax_ShouldThrow_AndKeepState()
    {
        // Arrange
        var sale = Sale.CreateNew(
            id: Guid.NewGuid(),
            number: "S-0001",
            customerId: Guid.NewGuid(),
            customerName: "Test Client",
            branchId: Guid.NewGuid(),
            branchName: "Branch Center");

        var productId = Guid.NewGuid();
        sale.AddItem(productId, "Lager beer", 10m, 3);

        // Act
        Action act = () => sale.UpdateItemQuantity(productId, DiscountPolicy.MaxPerItem + 1);

        // Assert
        var ex = act.Should().Throw<DomainException>().Which;
        ex.ErrorCode.Should().Be("MAX_PER_ITEM_EXCEEDED");
        ex.Message.Should().Be($"Quantity per product cannot exceed {DiscountPolicy.MaxPerItem}.");

        // Ensure state is maintained
        sale.ReadOnlyItems.Should().HaveCount(1);
        var item = sale.ReadOnlyItems.Single();
        item.Quantity.Should().Be(3);
    }

    [Fact(DisplayName = "Valid SetQuantity should update totals")]
    public void SetQuantity_Valid_ShouldUpdateTotals()
    {
        // Arrange
        var sale = Sale.CreateNew(
            id: Guid.NewGuid(),
            number: "S-0001",
            customerId: Guid.NewGuid(),
            customerName: "Test Client",
            branchId: Guid.NewGuid(),
            branchName: "Branch Center");

        var productId = Guid.NewGuid();
        sale.AddItem(productId, "Lager beer", 10m, 3);

        // Act
        sale.UpdateItemQuantity(productId, 5);

        // Assert
        sale.ReadOnlyItems.Should().HaveCount(1);
        var item = sale.ReadOnlyItems.Single();

        item.Quantity.Should().Be(5);
        item.UnitPrice.Should().Be(10m);
        item.DiscountAmount.Should().Be(5m);
        item.LineTotal.Should().Be(45m);

        // Sales totals reflecting the item
        sale.TotalAmount.Should().Be(50m);
        sale.TotalDiscount.Should().Be(5m);
        sale.TotalPayable.Should().Be(45m);
    }

    [Fact(DisplayName = "Same product with different price should fail")]
    public void SameProduct_DifferentPrice_ShouldThrow()
    {
        // Arrange
        var sale = Sale.CreateNew(
            id: Guid.NewGuid(),
            number: "S-0001",
            customerId: Guid.NewGuid(),
            customerName: "Test Client",
            branchId: Guid.NewGuid(),
            branchName: "Branch Center");

        var productId = Guid.NewGuid();
        sale.AddItem(productId, "Lager beer", 10m, 3);

        // Act
        Action act = () => sale.AddItem(productId, "Lager beer", 12m, 2);

        // Assert
        var ex = act.Should().Throw<DomainException>().Which;
        ex.ErrorCode.Should().Be("UNIT_PRICE_MISMATCH");
        ex.Message.Should().Be("Unit price does not match existing item price.");
    }

    // Cancelamento de item

    [Fact(DisplayName = "Cancel items: all active items are cancelled and totals zeroed")]
    public void CancelItem_ShouldExcludeFromTotals()
    {
        // Arrange
        var sale = Sale.CreateNew(
            id: Guid.NewGuid(),
            number: "S-0001",
            customerId: Guid.NewGuid(),
            customerName: "Test Client",
            branchId: Guid.NewGuid(),
            branchName: "Branch Center");

        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        sale.AddItem(productId1, "Lager beer", 10m, 5); // 10% discount
        sale.AddItem(productId2, "IPA beer", 15m, 2);   // no discount

        // Pre-check totals (gross/discount/total based on current policy)
        sale.TotalAmount.Should().Be(80m);
        sale.TotalDiscount.Should().Be(5m);
        sale.TotalPayable.Should().Be(75m);

        // Act
        sale.CancelItems();

        // Assert
        sale.ReadOnlyItems.Should().HaveCount(2);
        var item1 = sale.ReadOnlyItems.First(i => i.ProductId == productId1);
        var item2 = sale.ReadOnlyItems.First(i => i.ProductId == productId2);

        item1.IsCancelled.Should().BeTrue();
        item2.IsCancelled.Should().BeTrue();

        // Sales totals zeroed after cancelling all items
        sale.TotalAmount.Should().Be(0m);
        sale.TotalDiscount.Should().Be(0m);
        sale.TotalPayable.Should().Be(0m);
    }

    // Exceder limite ao incrementar a mesma linha

    [Fact(DisplayName = "Adding more units until exceeding limit should fail")]
    public void AddItem_ExceedMaxByIncrement_ShouldThrow()
    {
        // Arrange
        var sale = Sale.CreateNew(
            id: Guid.NewGuid(),
            number: "S-0001",
            customerId: Guid.NewGuid(),
            customerName: "Test Client",
            branchId: Guid.NewGuid(),
            branchName: "Branch Center");

        var productId = Guid.NewGuid();
        sale.AddItem(productId, "Lager beer", 10m, 3);

        // Act
        Action act = () => sale.AddItem(productId, "Lager beer", 10m, DiscountPolicy.MaxPerItem);

        // Assert
        var ex = act.Should().Throw<DomainException>().Which;
        ex.ErrorCode.Should().Be("MAX_PER_ITEM_EXCEEDED");
        ex.Message.Should().Be($"Quantity per product cannot exceed {DiscountPolicy.MaxPerItem}.");

        // Note: quantity was incremented before the validation throws
        sale.ReadOnlyItems.Should().HaveCount(1);
        var item = sale.ReadOnlyItems.Single();
        item.Quantity.Should().Be(3 + DiscountPolicy.MaxPerItem);
    }
}
