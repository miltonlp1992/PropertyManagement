using PropertyManagement.Application.DTOs;
using PropertyManagement.Application.Interfaces;
using PropertyManagement.Domain.Entities;
using PropertyManagement.Domain.Interfaces;
using PropertyManagement.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace PropertyManagement.Application.Services;

public class PropertyService : IPropertyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PropertyService> _logger;

    public PropertyService(
        IUnitOfWork unitOfWork,      
        IMapper mapper,
        ILogger<PropertyService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponseDto<PropertyDto>> GetByIdAsync(int id)
    {
        try
        {
            var property = await _unitOfWork.Properties.GetByIdAsync(id);
            if (property == null)
            {
                return ApiResponseDto<PropertyDto>.ErrorResponse("Property not found");
            }

            var propertyDto = _mapper.Map<PropertyDto>(property);
            return ApiResponseDto<PropertyDto>.SuccessResponse(propertyDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting property with id {PropertyId}", id);
            return ApiResponseDto<PropertyDto>.ErrorResponse("Error retrieving property");
        }
    }

    public async Task<ApiResponseDto<PagedResultDto<PropertyDto>>> GetFilteredAsync(PropertyFilterDto filterDto)
    {
        try
        {
            // Mapear el DTO de aplicación al modelo de dominio
            var domainFilter = _mapper.Map<PropertyFilter>(filterDto);

            // Usar el repositorio con el modelo de dominio
            var domainResult = await _unitOfWork.Properties.GetFilteredAsync(domainFilter);

            // Mapear el resultado del dominio al DTO de aplicación
            var propertyDtos = _mapper.Map<IEnumerable<PropertyDto>>(domainResult.Data);

            var pagedResult = new PagedResultDto<PropertyDto>
            {
                Data = propertyDtos,
                TotalCount = domainResult.TotalCount,
                PageNumber = domainResult.PageNumber,
                PageSize = domainResult.PageSize
            };

            return ApiResponseDto<PagedResultDto<PropertyDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting filtered properties");
            return ApiResponseDto<PagedResultDto<PropertyDto>>.ErrorResponse("Error retrieving properties");
        }
    }

    public async Task<ApiResponseDto<PropertyDto>> CreateAsync(CreatePropertyDto createPropertyDto)
    {
        try
        {
            // Validate owner exists
            if (!await _unitOfWork.Owners.ExistsAsync(createPropertyDto.IdOwner))
            {
                return ApiResponseDto<PropertyDto>.ErrorResponse("Owner not found");
            }

            var property = _mapper.Map<Property>(createPropertyDto);
            await _unitOfWork.Properties.CreateAsync(property);
            //await _unitOfWork.CompleteAsync();
            // Create property trace for creation
            var propertyTrace = new PropertyTrace
            {
                Property = property,
                //IdProperty = createdProperty.IdProperty,
                DateSale = DateTime.UtcNow,
                Name = "Property Created",
                Value = property.Price ?? 0,
                Tax = 0
            };

            await _unitOfWork.PropertyTraces.CreateAsync(propertyTrace);
            await _unitOfWork.CompleteAsync();

            var propertyDto = _mapper.Map<PropertyDto>(property);
            _logger.LogInformation("Property {PropertyId} created", propertyDto.IdProperty);
            return ApiResponseDto<PropertyDto>.SuccessResponse(propertyDto, "Property created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating property");
            return ApiResponseDto<PropertyDto>.ErrorResponse("Error creating property");
        }
    }

    public async Task<ApiResponseDto<PropertyDto>> UpdateAsync(int id, UpdatePropertyDto updatePropertyDto)
    {
        try
        {
            var existingProperty = await _unitOfWork.Properties.GetByIdAsync(id);
            if (existingProperty == null)
            {
                return ApiResponseDto<PropertyDto>.ErrorResponse("Property not found");
            }

            // Validate owner if provided
            if (updatePropertyDto.IdOwner.HasValue &&
                !await _unitOfWork.Owners.ExistsAsync(updatePropertyDto.IdOwner.Value))
            {
                return ApiResponseDto<PropertyDto>.ErrorResponse("Owner not found");
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(updatePropertyDto.Name))
                existingProperty.Name = updatePropertyDto.Name;

            if (!string.IsNullOrEmpty(updatePropertyDto.Address))
                existingProperty.Address = updatePropertyDto.Address;

            if (updatePropertyDto.Price.HasValue)
                existingProperty.Price = updatePropertyDto.Price;

            if (!string.IsNullOrEmpty(updatePropertyDto.CodeInternal))
                existingProperty.CodeInternal = updatePropertyDto.CodeInternal;

            if (updatePropertyDto.Year.HasValue)
                existingProperty.Year = updatePropertyDto.Year;

            if (updatePropertyDto.IdOwner.HasValue)
                existingProperty.IdOwner = updatePropertyDto.IdOwner.Value;

            if (updatePropertyDto.Enabled.HasValue)
                existingProperty.Enabled = updatePropertyDto.Enabled.Value;

            var updatedProperty = await _unitOfWork.Properties.UpdateAsync(existingProperty);
            var propertyDto = _mapper.Map<PropertyDto>(updatedProperty);

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Property {PropertyId} updated", id);
            return ApiResponseDto<PropertyDto>.SuccessResponse(propertyDto, "Property updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating property with id {PropertyId}", id);
            return ApiResponseDto<PropertyDto>.ErrorResponse("Error updating property");
        }
    }

    public async Task<ApiResponseDto<bool>> ChangePriceAsync(int id, decimal newPrice)
    {
        try
        {

            var property = await _unitOfWork.Properties.GetByIdAsync(id);
            if (property == null)
            {
                return ApiResponseDto<bool>.ErrorResponse("Property not found");
            }

            var oldPrice = property.Price ?? 0;
            property.Price = newPrice;
            await _unitOfWork.Properties.UpdateAsync(property);

            // Create property trace for price change
            var propertyTrace = new PropertyTrace
            {
                Property = property,
                DateSale = DateTime.UtcNow,
                Name = "Price Change",
                Value = newPrice,
                Tax = 0
            };
            await _unitOfWork.PropertyTraces.CreateAsync(propertyTrace);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Price changed for property {PropertyId} from {OldPrice} to {NewPrice}",
                id, oldPrice, newPrice);

            return ApiResponseDto<bool>.SuccessResponse(true, "Price updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing price for property {PropertyId}", id);
            return ApiResponseDto<bool>.ErrorResponse("Error updating price");
        }
    }

    public async Task<ApiResponseDto<bool>> DeleteAsync(int id)
    {
        try
        {
            var result = await _unitOfWork.Properties.DeleteAsync(id);
            if (!result)
            {
                return ApiResponseDto<bool>.ErrorResponse("Property not found");
            }

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Property {PropertyId} deleted", id);
            return ApiResponseDto<bool>.SuccessResponse(true, "Property deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting property with id {PropertyId}", id);
            return ApiResponseDto<bool>.ErrorResponse("Error deleting property");
        }
    }
}
