using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Sale?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .AsNoTracking()
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);
    }

    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        var saleExists = await _context.Sales
            .AsNoTracking()
            .AnyAsync(s => s.Id == sale.Id, cancellationToken);

        if (!saleExists)
            throw new KeyNotFoundException($"The sale with ID '{sale.Id}' was not found.");

        var existingItemIds = await _context.SaleItems
            .AsNoTracking()
            .Where(i => i.SaleId == sale.Id)
            .Select(i => i.Id)
            .ToListAsync(cancellationToken);

        var isFullItemReplace = !sale.Items.Any(i => existingItemIds.Contains(i.Id));

        if (isFullItemReplace)
        {
            await _context.Sales
                .Where(s => s.Id == sale.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.SaleDate, sale.SaleDate)
                    .SetProperty(s => s.CustomerId, sale.CustomerId)
                    .SetProperty(s => s.CustomerName, sale.CustomerName)
                    .SetProperty(s => s.BranchId, sale.BranchId)
                    .SetProperty(s => s.BranchName, sale.BranchName)
                    .SetProperty(s => s.TotalAmount, sale.TotalAmount)
                    .SetProperty(s => s.IsCancelled, sale.IsCancelled)
                    .SetProperty(s => s.CancelledAt, sale.CancelledAt)
                    .SetProperty(s => s.UpdatedAt, sale.UpdatedAt),
                cancellationToken);

            await _context.SaleItems
                .Where(i => i.SaleId == sale.Id)
                .ExecuteDeleteAsync(cancellationToken);

            foreach (var item in sale.Items)
            {
                await _context.SaleItems.AddAsync(new SaleItem
                {
                    SaleId = sale.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    DiscountPercentage = item.DiscountPercentage,
                    TotalAmount = item.TotalAmount,
                    IsCancelled = item.IsCancelled,
                    CancelledAt = item.CancelledAt
                }, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return (await GetByIdAsync(sale.Id, cancellationToken))!;
        }

        var existingSale = await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == sale.Id, cancellationToken);

        existingSale!.SaleDate = sale.SaleDate;
        existingSale.CustomerId = sale.CustomerId;
        existingSale.CustomerName = sale.CustomerName;
        existingSale.BranchId = sale.BranchId;
        existingSale.BranchName = sale.BranchName;
        existingSale.TotalAmount = sale.TotalAmount;
        existingSale.IsCancelled = sale.IsCancelled;
        existingSale.CancelledAt = sale.CancelledAt;
        existingSale.UpdatedAt = sale.UpdatedAt;

        foreach (var incomingItem in sale.Items)
        {
            var existingItem = existingSale.Items.FirstOrDefault(i => i.Id == incomingItem.Id);
                if (existingItem is null)
                continue;

            existingItem.ProductId = incomingItem.ProductId;
            existingItem.ProductName = incomingItem.ProductName;
            existingItem.Quantity = incomingItem.Quantity;
            existingItem.UnitPrice = incomingItem.UnitPrice;
            existingItem.DiscountPercentage = incomingItem.DiscountPercentage;
            existingItem.TotalAmount = incomingItem.TotalAmount;
            existingItem.IsCancelled = incomingItem.IsCancelled;
            existingItem.CancelledAt = incomingItem.CancelledAt;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return existingSale;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await GetByIdAsync(id, cancellationToken);
        if (sale is null)
            return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<(IReadOnlyList<Sale> Items, int TotalCount)> ListAsync(
        int page,
        int pageSize,
        string? orderBy,
        bool orderDescending,
        Guid? customerId,
        Guid? branchId,
        bool? isCancelled,
        DateTime? minSaleDate,
        DateTime? maxSaleDate,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sales
            .Include(s => s.Items)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(s => s.CustomerId == customerId.Value);

        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);

        if (isCancelled.HasValue)
            query = query.Where(s => s.IsCancelled == isCancelled.Value);

        if (minSaleDate.HasValue)
            query = query.Where(s => s.SaleDate >= minSaleDate.Value);

        if (maxSaleDate.HasValue)
            query = query.Where(s => s.SaleDate <= maxSaleDate.Value);

        query = ApplyOrdering(query, orderBy, orderDescending);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    private static IQueryable<Sale> ApplyOrdering(IQueryable<Sale> query, string? orderBy, bool orderDescending)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
            return orderDescending
                ? query.OrderByDescending(s => s.SaleDate)
                : query.OrderBy(s => s.SaleDate);

        var orderFields = orderBy.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        IOrderedQueryable<Sale>? orderedQuery = null;

        foreach (var field in orderFields)
        {
            var parts = field.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var property = parts[0].ToLowerInvariant();
            var descending = parts.Length > 1
                ? parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase)
                : orderDescending;

            orderedQuery = property switch
            {
                "salenumber" => ApplyOrder(orderedQuery, query, s => s.SaleNumber, descending),
                "saledate" => ApplyOrder(orderedQuery, query, s => s.SaleDate, descending),
                "totalamount" => ApplyOrder(orderedQuery, query, s => s.TotalAmount, descending),
                "customername" => ApplyOrder(orderedQuery, query, s => s.CustomerName, descending),
                "branchname" => ApplyOrder(orderedQuery, query, s => s.BranchName, descending),
                _ => ApplyOrder(orderedQuery, query, s => s.SaleDate, descending)
            };

            query = orderedQuery ?? query;
        }

        return orderedQuery ?? query.OrderBy(s => s.SaleDate);
    }

    private static IOrderedQueryable<Sale> ApplyOrder<TKey>(
        IOrderedQueryable<Sale>? orderedQuery,
        IQueryable<Sale> query,
        System.Linq.Expressions.Expression<Func<Sale, TKey>> keySelector,
        bool descending)
    {
        if (orderedQuery is null)
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);

        return descending
            ? orderedQuery.ThenByDescending(keySelector)
            : orderedQuery.ThenBy(keySelector);
    }
}
