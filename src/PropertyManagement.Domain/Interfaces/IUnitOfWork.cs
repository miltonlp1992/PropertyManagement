using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Repository Properties
    IPropertyRepository Properties { get; }
    IOwnerRepository Owners { get; }
    IPropertyImageRepository PropertyImages { get; }
    IPropertyTraceRepository PropertyTraces { get; }
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }

    // Transaction Methods
    Task<int> CompleteAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();

    // Bulk Operations
    Task<int> SaveChangesAsync();
    void DetachAllEntities();
}
