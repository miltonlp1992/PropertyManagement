using PropertyManagement.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Application.Interfaces;

public interface IPropertyTraceService
{
    Task<ApiResponseDto<IEnumerable<PropertyTraceDto>>> GetByPropertyIdAsync(int propertyId);
    Task<ApiResponseDto<PropertyTraceDto>> CreateAsync(CreatePropertyTraceDto createPropertyTraceDto);
    Task<ApiResponseDto<PropertyTraceDto>> GetByIdAsync(int id);
}
