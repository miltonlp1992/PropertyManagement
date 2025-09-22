using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PropertyManagement.Domain.Interfaces;
using PropertyManagement.Infrastructure.Data;
using PropertyManagement.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly PropertyManagementContext _context;
    private IDbContextTransaction? _transaction;

    // Repository instances
    private IPropertyRepository? _properties;
    private IOwnerRepository? _owners;
    private IPropertyImageRepository? _propertyImages;
    private IPropertyTraceRepository? _propertyTraces;
    private IUserRepository? _users;
    private IRefreshTokenRepository? _refreshTokens;

    public UnitOfWork(PropertyManagementContext context)
    {
        _context = context;
    }

    // Lazy initialization of repositories
    public IPropertyRepository Properties
    {
        get { return _properties ??= new PropertyRepository(_context); }
    }

    public IOwnerRepository Owners
    {
        get { return _owners ??= new OwnerRepository(_context); }
    }

    public IPropertyImageRepository PropertyImages
    {
        get { return _propertyImages ??= new PropertyImageRepository(_context); }
    }

    public IPropertyTraceRepository PropertyTraces
    {
        get { return _propertyTraces ??= new PropertyTraceRepository(_context); }
    }

    public IUserRepository Users
    {
        get { return _users ??= new UserRepository(_context); }
    }

    public IRefreshTokenRepository RefreshTokens
    {
        get { return _refreshTokens ??= new RefreshTokenRepository(_context); }
    }

    public async Task<int> CompleteAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log the exception
            throw new InvalidOperationException("An error occurred while saving changes to the database.", ex);
        }
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            try
            {
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void DetachAllEntities()
    {
        var entries = _context.ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Detached)
            .ToList();

        foreach (var entry in entries)
        {
            entry.State = EntityState.Detached;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
