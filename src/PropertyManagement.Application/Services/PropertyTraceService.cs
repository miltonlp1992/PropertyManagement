using AutoMapper;
using Microsoft.Extensions.Logging;
using PropertyManagement.Application.DTOs;
using PropertyManagement.Application.Interfaces;
using PropertyManagement.Domain.Entities;
using PropertyManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Application.Services;

public class PropertyTraceService : IPropertyTraceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PropertyTraceService> _logger;

    public PropertyTraceService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PropertyTraceService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponseDto<IEnumerable<PropertyTraceDto>>> GetByPropertyIdAsync(int propertyId)
    {
        try
        {
            // Use specific method from IPropertyTraceRepository
            var traces = await _unitOfWork.PropertyTraces.GetByPropertyIdAsync(propertyId);
            var traceDtos = _mapper.Map<IEnumerable<PropertyTraceDto>>(traces);
            return ApiResponseDto<IEnumerable<PropertyTraceDto>>.SuccessResponse(traceDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting traces for property {PropertyId}", propertyId);
            return ApiResponseDto<IEnumerable<PropertyTraceDto>>.ErrorResponse("Error retrieving property traces");
        }
    }

    public async Task<ApiResponseDto<PropertyTraceDto>> CreateAsync(CreatePropertyTraceDto createPropertyTraceDto)
    {
        try
        {
            // Validate property exists using generic method
            if (!await _unitOfWork.Properties.ExistsAsync(createPropertyTraceDto.IdProperty))
            {
                return ApiResponseDto<PropertyTraceDto>.ErrorResponse("Property not found");
            }

            await _unitOfWork.BeginTransactionAsync();

            var propertyTrace = _mapper.Map<PropertyTrace>(createPropertyTraceDto);
            var createdTrace = await _unitOfWork.PropertyTraces.CreateAsync(propertyTrace);

            await _unitOfWork.CompleteAsync();
            await _unitOfWork.CommitTransactionAsync();

            var traceDto = _mapper.Map<PropertyTraceDto>(createdTrace);

            _logger.LogInformation("Property trace created for property {PropertyId}", createPropertyTraceDto.IdProperty);
            return ApiResponseDto<PropertyTraceDto>.SuccessResponse(traceDto, "Property trace created successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error creating property trace");
            return ApiResponseDto<PropertyTraceDto>.ErrorResponse("Error creating property trace");
        }
    }

    public async Task<ApiResponseDto<PropertyTraceDto>> GetByIdAsync(int id)
    {
        try
        {
            // Use generic GetByIdAsync method
            var trace = await _unitOfWork.PropertyTraces.GetByIdAsync(id);
            if (trace == null)
            {
                return ApiResponseDto<PropertyTraceDto>.ErrorResponse("Property trace not found");
            }

            var traceDto = _mapper.Map<PropertyTraceDto>(trace);
            return ApiResponseDto<PropertyTraceDto>.SuccessResponse(traceDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting property trace with id {TraceId}", id);
            return ApiResponseDto<PropertyTraceDto>.ErrorResponse("Error retrieving property trace");
        }
    }
}
