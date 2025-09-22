using Microsoft.AspNetCore.Http;
using PropertyManagement.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Application.Interfaces;

public interface IPropertyImageService
{
    Task<ApiResponseDto<PropertyImageDto>> AddImageAsync(int propertyId, IFormFile file);
    Task<ApiResponseDto<IEnumerable<PropertyImageDto>>> GetImagesByPropertyIdAsync(int propertyId);
    Task<ApiResponseDto<bool>> DeleteImageAsync(int imageId);
    Task<ApiResponseDto<byte[]>> GetImageFileAsync(int imageId);
}
