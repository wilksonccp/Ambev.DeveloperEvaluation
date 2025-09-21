using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Policies;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.SaleTests;

public class Sale_UpdateItemQuantity_Tests
{
    [Fact]
    public void UpdateItemQuantity_should_move_between_tiers()
    {
        var sale = Sale.CreateNew(
            Guid.NewGuid(), "S-001",
            Guid.NewGuid(), "Cliente",
            Guid.NewGuid(), "Filial");
        var gui = Guid.NewGuid();

        sale.AddItem(gui, "Product A", 10.00m, 10); // 20% discount
        sale.TotalAmount.Should().Be(100.00m);
        sale.TotalPayable.Should().Be(80.00m);
        sale.TotalDiscount.Should().Be(20.00m);

        sale.UpdateItemQuantity(gui, 7); // now 7 items, 10% discount
        sale.TotalAmount.Should().Be(70.00m);
        sale.TotalPayable.Should().Be(63.00m);
        sale.TotalDiscount.Should().Be(7.00m);

        sale.UpdateItemQuantity(gui, 2); // now 2 items, no discount
        sale.TotalAmount.Should().Be(20.00m);
        sale.TotalPayable.Should().Be(20.00m);
        sale.TotalDiscount.Should().Be(0.00m);
    }

    [Fact]
    public void UpdateItemQuantity_to_zero_should_throw_and_keep_consistency()
    {
        var sale = Sale.CreateNew(
            Guid.NewGuid(), "S-002",
            Guid.NewGuid(), "Cliente",
            Guid.NewGuid(), "Filial");
        var gui = Guid.NewGuid();

        sale.AddItem(gui, "Product A", 10.00m, 5); // 10% discount
        sale.TotalAmount.Should().Be(50.00m);
        sale.TotalPayable.Should().Be(45.00m);
        sale.TotalDiscount.Should().Be(5.00m);

        Action act = () => sale.UpdateItemQuantity(gui, 0); // zero items, should throw
        var ex = act.Should().Throw<DomainException>().Which;
        ex.ErrorCode.Should().Be("QUANTITY_MUST_BE_POSITIVE");
        ex.Message.Should().Be("Quantity must be at least 1.");

        sale.TotalAmount.Should().Be(50.00m);
        sale.TotalPayable.Should().Be(45.00m);
        sale.TotalDiscount.Should().Be(5.00m);
    }

    [Fact]
    public void UpdateItemQuantity_item_not_found_should_throw()
    {
        var sale = Sale.CreateNew(
            Guid.NewGuid(), "S-010",
            Guid.NewGuid(), "Cliente",
            Guid.NewGuid(), "Filial");

        Action act = () => sale.UpdateItemQuantity(Guid.NewGuid(), 3);
        var ex = act.Should().Throw<DomainException>().Which;
        ex.ErrorCode.Should().Be("ITEM_NOT_FOUND");
        ex.Message.Should().Be("Cannot update quantity of a non-existing item.");
    }

    [Fact]
    public void UpdateItemQuantity_on_cancelled_sale_should_throw()
    {
        var sale = Sale.CreateNew(
            Guid.NewGuid(), "S-011",
            Guid.NewGuid(), "Cliente",
            Guid.NewGuid(), "Filial");
        var gui = Guid.NewGuid();
        sale.AddItem(gui, "Product A", 10.00m, 4);
        sale.CancelItems();
        sale.CancelSale();

        Action act = () => sale.UpdateItemQuantity(gui, 5);
        var ex = act.Should().Throw<DomainException>().Which;
        ex.ErrorCode.Should().Be("SALE_CANCELLED");
        ex.Message.Should().Be("Cannot modify a cancelled sale.");
    }

    [Fact]
    public void UpdateItemQuantity_over_MaxPerItem_should_throw_and_keep_state()
    {
        var sale = Sale.CreateNew(
            Guid.NewGuid(), "S-012",
            Guid.NewGuid(), "Cliente",
            Guid.NewGuid(), "Filial");
        var gui = Guid.NewGuid();
        sale.AddItem(gui, "Product A", 10.00m, 10);

        Action act = () => sale.UpdateItemQuantity(gui, DiscountPolicy.MaxPerItem + 1);
        var ex = act.Should().Throw<DomainException>().Which;
        ex.ErrorCode.Should().Be("MAX_PER_ITEM_EXCEEDED");
        ex.Message.Should().Be($"Quantity per product cannot exceed {DiscountPolicy.MaxPerItem}.");

        sale.TotalAmount.Should().Be(100.00m);
        sale.TotalPayable.Should().Be(80.00m);
        sale.TotalDiscount.Should().Be(20.00m);
    }
}
