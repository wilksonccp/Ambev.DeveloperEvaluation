using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.SaleTests
{
    public class Sale_RemoveItem_Tests
    {
        [Fact]
        public void RemoveItem_should_recalculate_total()
        {
            // Given
            var sale = Sale.CreateNew(
                Guid.NewGuid(), "S-003",
                Guid.NewGuid(), "Cliente",
                Guid.NewGuid(), "Filial");
            var gui1 = Guid.NewGuid();
            var gui2 = Guid.NewGuid();

            sale.AddItem(gui1, "Product A", 10.00m, 4); // 10% discount
            sale.AddItem(gui2, "Product B", 5.00m, 2); // no discount
            sale.TotalAmount.Should().Be(50.00m);
            sale.TotalPayable.Should().Be(46.00m);
            sale.TotalDiscount.Should().Be(4.00m);

            sale.RemoveItem(gui1, 2); // reduce Product A from 4 -> 2 (no discount)
            sale.TotalAmount.Should().Be(30.00m);
            sale.TotalPayable.Should().Be(30.00m);
            sale.TotalDiscount.Should().Be(0.00m);
        }
    }
        
}
   
