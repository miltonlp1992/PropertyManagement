using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Domain.Interfaces;

public interface IGenericRepository<T> where T : class
{
    // Basic CRUD Operations
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> expression);

    // Create Operations
    Task<T> CreateAsync(T entity);
    Task CreateRangeAsync(IEnumerable<T> entities);

    // Update Operations
    Task<T> UpdateAsync(T entity);
    void UpdateRange(IEnumerable<T> entities);

    // Delete Operations
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteAsync(object id);
    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);

    // Query Operations
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> expression);
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> expression);

    // Pagination
    Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);
    Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>> filter);
}
