using System.Collections.Generic;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.SaleTests
{
    public class Sale_CreateNew_Tests
    {
        public static IEnumerable<object[]> ValidCreateCases()
        {
            yield return new object[] { "S-0001", "Customer 01", "Branch 01" };
            yield return new object[] { "  S-0002  ", "  Customer 02  ", "  Branch 02  " }; // trims
        }

        [Theory]
        [MemberData(nameof(ValidCreateCases))]
        public void CreateNew_with_valid_data_should_succeed(string number, string customerName, string branchName)
        {
            // When
            var now = DateTime.UtcNow;
            var id = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var branchId = Guid.NewGuid();
            var sale = Sale.CreateNew(
                id, number,
                customerId, customerName,
                branchId, branchName);

            // Then
            sale.Id.Should().Be(id);
            sale.CreatedAt.Should().BeOnOrAfter(now.AddSeconds(-2));
            sale.UpdatedAt.Should().BeOnOrAfter(now.AddSeconds(-2));
            sale.DeletedAt.Should().BeNull();
            sale.Number.Should().Be(number.Trim());
            sale.CustomerId.Should().Be(customerId);
            sale.CustomerName.Should().Be(customerName.Trim());
            sale.BranchId.Should().Be(branchId);
            sale.BranchName.Should().Be(branchName.Trim());
            sale.IsCancelled.Should().BeFalse();
            sale.TotalAmount.Should().Be(0m);
            sale.TotalDiscount.Should().Be(0m);
            sale.TotalPayable.Should().Be(0m);
            sale.ReadOnlyItems.Should().BeEmpty();
        }

        public static IEnumerable<object[]> CreateWithItemsCases()
        {
            yield return new object[]
            {
                "S-0100", "Customer A", "Branch A",
                new List<(Guid productId, string productName, decimal unitPrice, int quantity)>
                {
                    (Guid.NewGuid(), "Product A", 10.00m, 4), // 10% off => 36
                    (Guid.NewGuid(), "Product B", 5.00m, 2),  // 0%  off => 10
                },
                50.00m, 46.00m, 4.00m
            };
        }

        [Theory]
        [MemberData(nameof(CreateWithItemsCases))]
        public void CreateNew_with_items_should_initialize_items_and_totals(
            string number, string customerName, string branchName,
            List<(Guid productId, string productName, decimal unitPrice, int quantity)> items,
            decimal expectedTotalAmount, decimal expectedTotalPayable, decimal expectedTotalDiscount)
        {
            // When
            var sale = Sale.CreateNew(
                Guid.NewGuid(), number,
                Guid.NewGuid(), customerName,
                Guid.NewGuid(), branchName,
                items);

            // Then
            sale.ReadOnlyItems.Should().HaveCount(2);
            sale.TotalAmount.Should().Be(expectedTotalAmount);
            sale.TotalPayable.Should().Be(expectedTotalPayable);
            sale.TotalDiscount.Should().Be(expectedTotalDiscount);
        }

        public static IEnumerable<object[]> InvalidCreateCases()
        {
            yield return new object[]
            {
                Guid.Empty, "S-ERR-1", Guid.NewGuid(), "Customer 01", Guid.NewGuid(), "Branch 01",
                "INVALID_SALE_ID", "Sale ID must be a valid GUID."
            };
            yield return new object[]
            {
                Guid.NewGuid(), "", Guid.NewGuid(), "Customer 01", Guid.NewGuid(), "Branch 01",
                "INVALID_SALE_NUMBER", "Sale number cannot be empty."
            };
            yield return new object[]
            {
                Guid.NewGuid(), "S-ERR-3", Guid.Empty, "Customer 01", Guid.NewGuid(), "Branch 01",
                "INVALID_CUSTOMER_ID", "Customer ID must be a valid GUID."
            };
            yield return new object[]
            {
                Guid.NewGuid(), "S-ERR-4", Guid.NewGuid(), "", Guid.NewGuid(), "Branch 01",
                "INVALID_CUSTOMER_NAME", "Customer name cannot be empty."
            };
            yield return new object[]
            {
                Guid.NewGuid(), "S-ERR-5", Guid.NewGuid(), "Customer 01", Guid.Empty, "Branch 01",
                "INVALID_BRANCH_ID", "Branch ID must be a valid GUID."
            };
            yield return new object[]
            {
                Guid.NewGuid(), "S-ERR-6", Guid.NewGuid(), "Customer 01", Guid.NewGuid(), "",
                "INVALID_BRANCH_NAME", "Branch name cannot be empty."
            };
        }

        [Theory]
        [MemberData(nameof(InvalidCreateCases))]
        public void CreateNew_with_invalid_data_should_throw(
            Guid id, string number,
            Guid customerId, string customerName,
            Guid branchId, string branchName,
            string expectedCode, string expectedMessage)
        {
            // When
            Action act = () => Sale.CreateNew(id, number, customerId, customerName, branchId, branchName);

            // Then
            var ex = act.Should().Throw<DomainException>().Which;
            ex.ErrorCode.Should().Be(expectedCode);
            ex.Message.Should().Be(expectedMessage);
        }
    }
}
