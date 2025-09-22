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

public class PropertyTraceRepository : GenericRepository<PropertyTrace>, IPropertyTraceRepository
{
    public PropertyTraceRepository(PropertyManagementContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PropertyTrace>> GetByPropertyIdAsync(int propertyId)
    {
        return await _dbSet
            .Where(pt => pt.IdProperty == propertyId)
            .OrderByDescending(pt => pt.DateSale)
            .ToListAsync();
    }
}
