using PropertyManagement.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Application.Interfaces;

public interface IPropertyService
{
    Task<ApiResponseDto<PropertyDto>> GetByIdAsync(int id);
    Task<ApiResponseDto<PagedResultDto<PropertyDto>>> GetFilteredAsync(PropertyFilterDto filter);
    Task<ApiResponseDto<PropertyDto>> CreateAsync(CreatePropertyDto createPropertyDto);
    Task<ApiResponseDto<PropertyDto>> UpdateAsync(int id, UpdatePropertyDto updatePropertyDto);
    Task<ApiResponseDto<bool>> ChangePriceAsync(int id, decimal newPrice);
    Task<ApiResponseDto<bool>> DeleteAsync(int id);
}