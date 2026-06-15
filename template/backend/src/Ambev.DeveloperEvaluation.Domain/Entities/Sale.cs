using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a sale aggregate root.
/// </summary>
public class Sale : BaseEntity
{
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<SaleItem> Items { get; set; } = [];

    public Sale()
    {
        CreatedAt = DateTime.UtcNow;
        SaleDate = DateTime.UtcNow;
    }

    public static Sale Create(
        string saleNumber,
        DateTime saleDate,
        ExternalIdentity customer,
        ExternalIdentity branch,
        IEnumerable<(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice)> items)
    {
        var sale = new Sale
        {
            SaleNumber = saleNumber,
            SaleDate = saleDate,
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            BranchId = branch.Id,
            BranchName = branch.Name
        };

        sale.SetItems(items);
        sale.RecalculateTotalAmount();
        return sale;
    }

    public void Update(
        DateTime saleDate,
        ExternalIdentity customer,
        ExternalIdentity branch,
        IEnumerable<(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice)> items)
    {
        if (IsCancelled)
            throw new DomainException(
                $"Cannot update sale '{SaleNumber}' (ID {Id}) because it is already cancelled.");

        SaleDate = saleDate;
        CustomerId = customer.Id;
        CustomerName = customer.Name;
        BranchId = branch.Id;
        BranchName = branch.Name;

        SetItems(items);
        RecalculateTotalAmount();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (IsCancelled)
            throw new DomainException($"Sale '{SaleNumber}' (ID {Id}) is already cancelled.");

        IsCancelled = true;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CancelItem(Guid itemId)
    {
        if (IsCancelled)
            throw new DomainException(
                $"Cannot cancel items on cancelled sale '{SaleNumber}' (ID {Id}).");

        var item = Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException(
                $"Sale item with ID '{itemId}' was not found in sale '{SaleNumber}' (ID {Id}).");

        item.Cancel();
        RecalculateTotalAmount();
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetItems(IEnumerable<(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice)> items)
    {
        var itemList = items.ToList();

        if (itemList.Count == 0)
            throw new DomainException(
                $"Sale '{SaleNumber}' (ID {Id}) must contain at least one item.");

        Items = itemList
            .Select(i => SaleItem.Create(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice))
            .ToList();

        foreach (var item in Items)
            item.SaleId = Id;
    }

    public void RecalculateTotalAmount()
    {
        TotalAmount = Items
            .Where(i => !i.IsCancelled)
            .Sum(i => i.TotalAmount);
    }
}
