using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleItemTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(3, 0)]
    [InlineData(4, 10)]
    [InlineData(9, 10)]
    [InlineData(10, 20)]
    [InlineData(20, 20)]
    public void CalculateDiscountPercentage_ShouldApplyBusinessRules(int quantity, decimal expectedDiscount)
    {
        SaleItem.CalculateDiscountPercentage(quantity).Should().Be(expectedDiscount);
    }

    [Fact]
    public void Create_WithMoreThanTwentyItems_ShouldThrow()
    {
        var act = () => SaleItem.Create(Guid.NewGuid(), "Product", 21, 10m);

        act.Should().Throw<Ambev.DeveloperEvaluation.Domain.Exceptions.DomainException>()
            .WithMessage("*20*");
    }

    [Fact]
    public void Create_WithValidQuantity_ShouldCalculateTotalWithDiscount()
    {
        var item = SaleItem.Create(Guid.NewGuid(), "Product", 10, 100m);

        item.DiscountPercentage.Should().Be(20m);
        item.TotalAmount.Should().Be(800m);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrow()
    {
        var item = SaleItem.Create(Guid.NewGuid(), "Product", 2, 50m);
        item.Cancel();

        var act = () => item.Cancel();

        act.Should().Throw<Ambev.DeveloperEvaluation.Domain.Exceptions.DomainException>();
    }
}
