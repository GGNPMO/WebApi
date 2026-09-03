using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Interfaces;

namespace Payroll.Infrastructure.Persistence;

public class Repository<T>(PayrollDbContext context) : IRepository<T> where T : class
{
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _dbSet.FindAsync([id], ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await _dbSet.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await _dbSet.Where(predicate).ToListAsync(ct);

    // Pagination:
    public async Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<T, bool>>? predicate,
        int pageNumber,
        int pageSize,
        Func<IQueryable<T>, IQueryable<T>>? queryShaper = null,
        CancellationToken ct = default)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();
        if (predicate is not null)
            query = query.Where(predicate);
        var totalCount = await query.CountAsync(ct);
        if (queryShaper is not null)
            query = queryShaper(query);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items.AsReadOnly(), totalCount);
    }

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        return entity;
    }

    public Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }
}
