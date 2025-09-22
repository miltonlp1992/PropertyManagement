using PropertyManagement.Domain.Entities;
using PropertyManagement.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Domain.Interfaces;

public interface IPropertyRepository : IGenericRepository<Property>
{
    Task<PagedResult<Property>> GetFilteredAsync(PropertyFilter filter);    
}
