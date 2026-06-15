using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// Implementation of IUserRepository using Entity Framework Core
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly DefaultContext _context;

    /// <summary>
    /// Initializes a new instance of UserRepository
    /// </summary>
    /// <param name="context">The database context</param>
    public UserRepository(DefaultContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new user in the database
    /// </summary>
    /// <param name="user">The user to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created user</returns>
    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    /// <summary>
    /// Retrieves a user by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user if found, null otherwise</returns>
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(o=> o.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves a user by their email address
    /// </summary>
    /// <param name="email">The email address to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user if found, null otherwise</returns>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    /// <summary>
    /// Deletes a user from the database
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the user was deleted, false if not found</returns>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user is null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> ListAsync(
        int page,
        int pageSize,
        string? orderBy,
        bool orderDescending,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();
        query = ApplyOrdering(query, orderBy, orderDescending);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    private static IQueryable<User> ApplyOrdering(IQueryable<User> query, string? orderBy, bool orderDescending)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
            return orderDescending
                ? query.OrderByDescending(u => u.Username)
                : query.OrderBy(u => u.Username);

        var orderFields = orderBy.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        IOrderedQueryable<User>? orderedQuery = null;

        foreach (var field in orderFields)
        {
            var parts = field.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var property = parts[0].ToLowerInvariant();
            var descending = parts.Length > 1
                ? parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase)
                : orderDescending;

            orderedQuery = property switch
            {
                "username" => ApplyOrder(orderedQuery, query, u => u.Username, descending),
                "email" => ApplyOrder(orderedQuery, query, u => u.Email, descending),
                "phone" => ApplyOrder(orderedQuery, query, u => u.Phone, descending),
                "status" => ApplyOrder(orderedQuery, query, u => u.Status, descending),
                "role" => ApplyOrder(orderedQuery, query, u => u.Role, descending),
                "createdat" => ApplyOrder(orderedQuery, query, u => u.CreatedAt, descending),
                _ => ApplyOrder(orderedQuery, query, u => u.Username, descending)
            };

            query = orderedQuery ?? query;
        }

        return orderedQuery ?? query.OrderBy(u => u.Username);
    }

    private static IOrderedQueryable<User> ApplyOrder<TKey>(
        IOrderedQueryable<User>? orderedQuery,
        IQueryable<User> query,
        System.Linq.Expressions.Expression<Func<User, TKey>> keySelector,
        bool descending)
    {
        if (orderedQuery is null)
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);

        return descending
            ? orderedQuery.ThenByDescending(keySelector)
            : orderedQuery.ThenBy(keySelector);
    }
}
