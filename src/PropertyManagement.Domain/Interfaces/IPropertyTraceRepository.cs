using PropertyManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Domain.Interfaces;

public interface IPropertyTraceRepository : IGenericRepository<PropertyTrace>
{
    // Métodos específicos para PropertyTrace
    Task<IEnumerable<PropertyTrace>> GetByPropertyIdAsync(int propertyId);
    
}
