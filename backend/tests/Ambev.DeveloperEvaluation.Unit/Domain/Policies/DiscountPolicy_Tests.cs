using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Policies;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Policies
{
    public class DiscountPolicy_Tests
    {
        [Theory]
        [InlineData(1, 0.00)]
        [InlineData(3, 0.00)]
        [InlineData(4, 0.10)]
        [InlineData(9, 0.10)]
        [InlineData(10, 0.20)]
        [InlineData(20, 0.20)]

        public void GetRate_should_return_expected_rates(int qty, double expectedRate)
        {
            // Act
            var rate = DiscountPolicy.GetRate(qty);
            // Assert
            rate.Should().Be((decimal)expectedRate);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void GetRate_should_throw_when_qty_invalid(int qty)
        {
            // Act
            Action act = () => DiscountPolicy.GetRate(qty);
            // Assert
            var ex = act.Should().Throw<DomainException>().Which;
            ex.ErrorCode.Should().Be("QUANTITY_MUST_BE_POSITIVE");
            ex.Message.Should().Be("Quantity must be at least 1.");
        }

        [Fact]
        public void GetRate_should_throw_when_qty_exceeds_20()
        {
            // Given
            Action act = () => DiscountPolicy.GetRate(21);
            // When

            // Then
            var ex = act.Should().Throw<DomainException>().Which;
            ex.ErrorCode.Should().Be("MAX_PER_ITEM_EXCEEDED");
            ex.Message.Should().Be($"Quantity per product cannot exceed {DiscountPolicy.MaxPerItem}.");
        }

        [Theory]
        [InlineData(10.00, 3, 0.00, 30.00)]
        [InlineData(10.00, 4, 4.00, 36.00)]
        [InlineData(10.00, 10, 20.00, 80.00)]
        [InlineData(4.5, 6, 2.70, 24.30)]
        public void Calculate_should_return_discount_and_line_total(double unitPrice, int qty, double expectedDiscount, double expectedTotal)
        {
            // Act
            var (discount, lineTotal) = DiscountPolicy.CalculateDiscount((decimal)unitPrice, qty);
            // Assert
            discount.Should().Be((decimal)expectedDiscount);
            lineTotal.Should().Be((decimal)expectedTotal);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Calculate_should_throw_when_unit_price_invalid(decimal unitPrice)
        {
            // Given
            Action act = () => DiscountPolicy.CalculateDiscount(unitPrice, 1);
            // When / Then
            var ex = act.Should().Throw<DomainException>().Which;
            ex.ErrorCode.Should().Be("UNIT_PRICE_MUST_BE_POSITIVE");
            ex.Message.Should().Be("Unit price must be greater than 0.");
        }
    }
}
