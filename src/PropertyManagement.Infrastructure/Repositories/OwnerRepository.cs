using PropertyManagement.Domain.Entities;
using PropertyManagement.Domain.Interfaces;
using PropertyManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Infrastructure.Repositories;

public class OwnerRepository : GenericRepository<Owner>, IOwnerRepository
{
    public OwnerRepository(PropertyManagementContext context) : base(context)
    {
    }

    public override async Task<Owner?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(o => o.Properties)
            .FirstOrDefaultAsync(o => o.IdOwner == id);
    }

    public override async Task<IEnumerable<Owner>> GetAllAsync()
    {
        return await _dbSet
            .Include(o => o.Properties)
            .ToListAsync();
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        var owner = await GetByIdAsync(id);
        if (owner == null)
            return false;

        // Check if owner has properties
        var hasProperties = await _context.Properties
            .AnyAsync(p => p.IdOwner == id && p.Enabled);

        if (hasProperties)
        {
            throw new InvalidOperationException("Cannot delete owner with active properties");
        }

        _dbSet.Remove(owner);
        return true;
    }
}
