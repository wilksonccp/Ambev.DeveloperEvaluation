using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.SaleTests;

public class Sale_Cancel_Tests
{
    [Fact]
    public void Cancelled_sale_cannot_be_modified()
    {
        // Given
        var sale = Sale.CreateNew(
            Guid.NewGuid(), "S-001",
            Guid.NewGuid(), "Cliente",
            Guid.NewGuid(), "Filial");
        var gui = Guid.NewGuid();

        sale.AddItem(gui, "Product A", 10.00m, 5); // 10% discount
        sale.TotalAmount.Should().Be(50.00m);
        sale.TotalPayable.Should().Be(45.00m);
        sale.TotalDiscount.Should().Be(5.00m);

        // When
        sale.CancelItems();
        sale.CancelSale();

        // Then
        Action act1 = () => sale.AddItem(Guid.NewGuid(), "Product B", 20.00m, 2);
        var ex1 = act1.Should().Throw<DomainException>().Which;
        ex1.ErrorCode.Should().Be("SALE_CANCELLED");
        ex1.Message.Should().Be("Cannot modify a cancelled sale.");

        Action act2 = () => sale.UpdateItemQuantity(gui, 10);
        var ex2 = act2.Should().Throw<DomainException>().Which;
        ex2.ErrorCode.Should().Be("SALE_CANCELLED");
        ex2.Message.Should().Be("Cannot modify a cancelled sale.");

        Action act3 = () => sale.RemoveItem(gui, 1);
        var ex3 = act3.Should().Throw<DomainException>().Which;
        ex3.ErrorCode.Should().Be("SALE_CANCELLED");
        ex3.Message.Should().Be("Cannot modify a cancelled sale.");

        // Consistency check after cancellation: totals zeroed
        sale.TotalAmount.Should().Be(0.00m);
        sale.TotalPayable.Should().Be(0.00m);
        sale.TotalDiscount.Should().Be(0.00m);
    }
}
