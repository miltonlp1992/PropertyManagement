using PropertyManagement.Domain.Entities;
using PropertyManagement.Domain.Interfaces;
using PropertyManagement.Domain.Models;
using PropertyManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Infrastructure.Repositories;

public class PropertyRepository : GenericRepository<Property>, IPropertyRepository
{
    public PropertyRepository(PropertyManagementContext context) : base(context)
    {
    }

    public override async Task<Property?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Owner)
            .Include(p => p.PropertyImages.Where(pi => pi.Enabled))
            .Include(p => p.PropertyTraces)
            .FirstOrDefaultAsync(p => p.IdProperty == id);
    }

    public async Task<PagedResult<Property>> GetFilteredAsync(PropertyFilter filter)
    {
        var query = _dbSet
            .Include(p => p.Owner)
            .Include(p => p.PropertyImages.Where(pi => pi.Enabled))
            .Include(p => p.PropertyTraces)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(filter.Name))
            query = query.Where(p => p.Name.Contains(filter.Name));

        if (!string.IsNullOrEmpty(filter.Address))
            query = query.Where(p => p.Address.Contains(filter.Address));

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice);

        if (filter.MinYear.HasValue)
            query = query.Where(p => p.Year >= filter.MinYear);

        if (filter.MaxYear.HasValue)
            query = query.Where(p => p.Year <= filter.MaxYear);

        if (filter.IdOwner.HasValue)
            query = query.Where(p => p.IdOwner == filter.IdOwner);

        if (filter.Enabled.HasValue)
            query = query.Where(p => p.Enabled == filter.Enabled);

        var totalCount = await query.CountAsync();

        var properties = await query
            .OrderBy(p => p.Name)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<Property>
        {
            Data = properties,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        var property = await GetByIdAsync(id);
        if (property == null)
            return false;

        property.Enabled = false; // Soft delete
        await UpdateAsync(property);
        return true;
    }
}
