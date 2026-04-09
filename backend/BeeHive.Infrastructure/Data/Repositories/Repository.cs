using System.Linq.Expressions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BeeHive.Infrastructure.Data.Repositories;

/// <summary>
/// Generic EF Core repository providing common CRUD operations.
/// Concrete repositories extend this and add domain-specific queries.
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly BeeHiveDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(BeeHiveDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync() =>
        await _dbSet.AsNoTracking().ToListAsync();

    public async Task<T?> GetByIdAsync(int id) =>
        await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.AsNoTracking().Where(predicate).ToListAsync();

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _dbSet.FindAsync(id) is not null;
}
