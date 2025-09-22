using PropertyManagement.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Application.Interfaces;

public interface IOwnerService
{
    Task<ApiResponseDto<OwnerDto>> GetByIdAsync(int id);
    Task<ApiResponseDto<IEnumerable<OwnerDto>>> GetAllAsync();
    Task<ApiResponseDto<OwnerDto>> CreateAsync(CreateOwnerDto createOwnerDto);
    Task<ApiResponseDto<OwnerDto>> UpdateAsync(int id, UpdateOwnerDto updateOwnerDto);
    Task<ApiResponseDto<bool>> DeleteAsync(int id);
}
