using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a line item within a sale.
/// </summary>
public class SaleItem : BaseEntity
{
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime? CancelledAt { get; set; }

    public Sale? Sale { get; set; }

    public static SaleItem Create(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        if (quantity > 20)
            throw new DomainException(
                $"Cannot sell more than 20 identical items of product '{productName}' (ID {productId}); requested quantity: {quantity}.");

        if (quantity <= 0)
            throw new DomainException(
                $"Item quantity must be greater than zero for product '{productName}' (ID {productId}); received: {quantity}.");

        if (unitPrice <= 0)
            throw new DomainException(
                $"Unit price must be greater than zero for product '{productName}' (ID {productId}); received: {unitPrice}.");

        var discountPercentage = CalculateDiscountPercentage(quantity);
        var totalAmount = CalculateTotalAmount(quantity, unitPrice, discountPercentage);

        return new SaleItem
        {
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice,
            DiscountPercentage = discountPercentage,
            TotalAmount = totalAmount,
            IsCancelled = false
        };
    }

    public void Update(int quantity, decimal unitPrice)
    {
        if (IsCancelled)
            throw new DomainException(
                $"Cannot update cancelled item '{ProductName}' (item ID {Id}, product ID {ProductId}) on sale ID {SaleId}.");

        if (quantity > 20)
            throw new DomainException(
                $"Cannot sell more than 20 identical items of product '{ProductName}' (ID {ProductId}); requested quantity: {quantity}.");

        if (quantity <= 0)
            throw new DomainException(
                $"Item quantity must be greater than zero for product '{ProductName}' (ID {ProductId}); received: {quantity}.");

        if (unitPrice <= 0)
            throw new DomainException(
                $"Unit price must be greater than zero for product '{ProductName}' (ID {ProductId}); received: {unitPrice}.");

        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountPercentage = CalculateDiscountPercentage(quantity);
        TotalAmount = CalculateTotalAmount(quantity, unitPrice, DiscountPercentage);
    }

    public void Cancel()
    {
        if (IsCancelled)
            throw new DomainException(
                $"Item '{ProductName}' (item ID {Id}) is already cancelled on sale ID {SaleId}.");

        IsCancelled = true;
        CancelledAt = DateTime.UtcNow;
    }

    public static decimal CalculateDiscountPercentage(int quantity)
    {
        if (quantity < 4)
            return 0m;

        if (quantity >= 10)
            return 20m;

        return 10m;
    }

    public static decimal CalculateTotalAmount(int quantity, decimal unitPrice, decimal discountPercentage)
    {
        var grossAmount = quantity * unitPrice;
        return Math.Round(grossAmount * (1 - discountPercentage / 100m), 2, MidpointRounding.AwayFromZero);
    }
}
