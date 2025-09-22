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

public class PropertyImageRepository : GenericRepository<PropertyImage>, IPropertyImageRepository
{
    public PropertyImageRepository(PropertyManagementContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PropertyImage>> GetByPropertyIdAsync(int propertyId)
    {
        return await _dbSet
            .Where(pi => pi.IdProperty == propertyId && pi.Enabled)
            .ToListAsync();
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        var image = await GetByIdAsync(id);
        if (image == null)
            return false;

        image.Enabled = false; // Soft delete
        await UpdateAsync(image);
        return true;
    }

    public async Task<IEnumerable<PropertyImage>> GetEnabledByPropertyIdAsync(int propertyId)
    {
        return await _dbSet
            .Where(pi => pi.IdProperty == propertyId && pi.Enabled)
            .OrderBy(pi => pi.IdPropertyImage)
            .ToListAsync();
    }
}
