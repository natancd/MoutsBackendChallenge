using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Fact]
    public void Create_ShouldCalculateTotalFromItems()
    {
        var customer = new ExternalIdentity(Guid.NewGuid(), "Customer A");
        var branch = new ExternalIdentity(Guid.NewGuid(), "Branch A");
        var items = new[]
        {
            (Guid.NewGuid(), "Product A", 2, 100m),
            (Guid.NewGuid(), "Product B", 10, 50m)
        };

        var sale = Sale.Create("SALE-001", DateTime.UtcNow, customer, branch, items);

        sale.TotalAmount.Should().Be(600m);
        sale.Items.Should().HaveCount(2);
        sale.IsCancelled.Should().BeFalse();
    }

    [Fact]
    public void CancelItem_ShouldRecalculateTotalExcludingCancelledItem()
    {
        var customer = new ExternalIdentity(Guid.NewGuid(), "Customer A");
        var branch = new ExternalIdentity(Guid.NewGuid(), "Branch A");
        var productId = Guid.NewGuid();
        var items = new[]
        {
            (productId, "Product A", 2, 100m),
            (Guid.NewGuid(), "Product B", 4, 50m)
        };

        var sale = Sale.Create("SALE-002", DateTime.UtcNow, customer, branch, items);
        var itemToCancel = sale.Items.First(i => i.ProductId == productId);

        sale.CancelItem(itemToCancel.Id);

        sale.TotalAmount.Should().Be(180m);
        itemToCancel.IsCancelled.Should().BeTrue();
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrow()
    {
        var customer = new ExternalIdentity(Guid.NewGuid(), "Customer A");
        var branch = new ExternalIdentity(Guid.NewGuid(), "Branch A");
        var items = new[] { (Guid.NewGuid(), "Product A", 1, 10m) };
        var sale = Sale.Create("SALE-003", DateTime.UtcNow, customer, branch, items);

        sale.Cancel();

        var act = () => sale.Cancel();

        act.Should().Throw<Ambev.DeveloperEvaluation.Domain.Exceptions.DomainException>();
    }
}
