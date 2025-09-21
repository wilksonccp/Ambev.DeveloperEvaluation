using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Policies;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.SaleTests
{
    public class Sale_AddItem_Tests
    {
        [Fact]
        public void AddItem_should_create_new_line_and_update_total()
        {
            // Given
            var sale = Sale.CreateNew(
                Guid.NewGuid(), "S-001",
                Guid.NewGuid(), "Cliente",
                Guid.NewGuid(), "Filial");

            sale.AddItem(Guid.NewGuid(), "Product A", 10.00m, 4); // 10% discount
            sale.TotalAmount.Should().Be(40.00m);
            sale.TotalPayable.Should().Be(36.00m);
            sale.TotalDiscount.Should().Be(4.00m);

            sale.AddItem(Guid.NewGuid(), "Product B", 5.00m, 2); // no discount
            sale.TotalAmount.Should().Be(50.00m);
            sale.TotalPayable.Should().Be(46.00m);
            sale.TotalDiscount.Should().Be(4.00m);
        }

        [Fact]
        public void AddItem_same_product_same_price_should_increase_qty_and_recalculate_tier()
        {
            // Given
            var sale = Sale.CreateNew(
                Guid.NewGuid(), "S-002",
                Guid.NewGuid(), "Cliente",
                Guid.NewGuid(), "Filial");
            var gui = Guid.NewGuid();

            sale.AddItem(gui, "Product A", 10.00m, 3); // no discount
            sale.TotalAmount.Should().Be(30.00m);
            sale.TotalPayable.Should().Be(30.00m);
            sale.TotalDiscount.Should().Be(0.00m);

            sale.AddItem(gui, "Product A", 10.00m, 2); // now 5 items, 10% discount
            sale.TotalAmount.Should().Be(50.00m); // gross
            sale.TotalPayable.Should().Be(45.00m); // net
            sale.TotalDiscount.Should().Be(5.00m);

            sale.AddItem(gui, "Product A", 10.00m, 5); // now 10 items, 20% discount
            sale.TotalAmount.Should().Be(100.00m); // gross
            sale.TotalPayable.Should().Be(80.00m); // net
            sale.TotalDiscount.Should().Be(20.00m);
        }

        [Fact]

        public void AddItem_same_product_different_price_should_throw()
        {
            // Given
            var sale = Sale.CreateNew(
                Guid.NewGuid(), "S-003",
                Guid.NewGuid(), "Cliente",
                Guid.NewGuid(), "Filial");
            var gui = Guid.NewGuid();

            sale.AddItem(gui, "Product A", 10.00m, 3); // no discount
            sale.TotalAmount.Should().Be(30.00m);

            // Act
            Action act = () => sale.AddItem(gui, "Product A", 9.00m, 2); // different price

            // Then
            var ex = act.Should().Throw<DomainException>().Which;
            ex.ErrorCode.Should().Be("UNIT_PRICE_MISMATCH");
            ex.Message.Should().Be("Unit price does not match existing item price.");
        }
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void AddItem_with_invalid_quantity_should_throw(int qty)
        {
            // Given
            var sale = Sale.CreateNew(
                Guid.NewGuid(), "S-004",
                Guid.NewGuid(), "Cliente",
                Guid.NewGuid(), "Filial");
            var gui = Guid.NewGuid();

            // Act
            Action act = () => sale.AddItem(gui, "Product A", 10.00m, qty);

            // Then
            var ex = act.Should().Throw<DomainException>().Which;
            ex.ErrorCode.Should().Be("QUANTITY_MUST_BE_POSITIVE");
            ex.Message.Should().Be("Quantity must be at least 1.");
        }

        [Fact]
        public void AddItem_over_20_should_throw()
        {
            // Given
            var sale = Sale.CreateNew(
                Guid.NewGuid(), "S-005",
                Guid.NewGuid(), "Cliente",
                Guid.NewGuid(), "Filial");
            var gui = Guid.NewGuid();

            // Act
            Action act = () => sale.AddItem(gui, "Product A", 10.00m, 21);

            // Then
            var ex = act.Should().Throw<DomainException>().Which;
            ex.ErrorCode.Should().Be("MAX_PER_ITEM_EXCEEDED");
            ex.Message.Should().Be($"Quantity per product cannot exceed {DiscountPolicy.MaxPerItem}.");
        }
    }   
}

    
