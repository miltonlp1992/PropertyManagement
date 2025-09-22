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

public class OwnerService : IOwnerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OwnerService> _logger;

    public OwnerService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<OwnerService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponseDto<OwnerDto>> GetByIdAsync(int id)
    {
        try
        {
            // Use overridden GetByIdAsync that includes properties
            var owner = await _unitOfWork.Owners.GetByIdAsync(id);
            if (owner == null)
            {
                return ApiResponseDto<OwnerDto>.ErrorResponse("Owner not found");
            }

            var ownerDto = _mapper.Map<OwnerDto>(owner);
            return ApiResponseDto<OwnerDto>.SuccessResponse(ownerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting owner with id {OwnerId}", id);
            return ApiResponseDto<OwnerDto>.ErrorResponse("Error retrieving owner");
        }
    }

    public async Task<ApiResponseDto<IEnumerable<OwnerDto>>> GetAllAsync()
    {
        try
        {
            // Use overridden GetAllAsync that includes properties
            var owners = await _unitOfWork.Owners.GetAllAsync();
            var ownerDtos = _mapper.Map<IEnumerable<OwnerDto>>(owners);
            return ApiResponseDto<IEnumerable<OwnerDto>>.SuccessResponse(ownerDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all owners");
            return ApiResponseDto<IEnumerable<OwnerDto>>.ErrorResponse("Error retrieving owners");
        }
    }

    public async Task<ApiResponseDto<OwnerDto>> CreateAsync(CreateOwnerDto createOwnerDto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var owner = _mapper.Map<Owner>(createOwnerDto);
            var createdOwner = await _unitOfWork.Owners.CreateAsync(owner);

            await _unitOfWork.CompleteAsync();
            await _unitOfWork.CommitTransactionAsync();

            var ownerDto = _mapper.Map<OwnerDto>(createdOwner);

            _logger.LogInformation("Owner {OwnerName} created with ID {OwnerId}", owner.Name, createdOwner.IdOwner);
            return ApiResponseDto<OwnerDto>.SuccessResponse(ownerDto, "Owner created successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error creating owner");
            return ApiResponseDto<OwnerDto>.ErrorResponse("Error creating owner");
        }
    }

    public async Task<ApiResponseDto<OwnerDto>> UpdateAsync(int id, UpdateOwnerDto updateOwnerDto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var existingOwner = await _unitOfWork.Owners.GetByIdAsync(id);
            if (existingOwner == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponseDto<OwnerDto>.ErrorResponse("Owner not found");
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateOwnerDto.Name))
                existingOwner.Name = updateOwnerDto.Name;

            if (!string.IsNullOrEmpty(updateOwnerDto.Address))
                existingOwner.Address = updateOwnerDto.Address;

            if (updateOwnerDto.Birthday.HasValue)
                existingOwner.Birthday = updateOwnerDto.Birthday;

            if (!string.IsNullOrEmpty(updateOwnerDto.PhotoBase64))
                existingOwner.Photo = Convert.FromBase64String(updateOwnerDto.PhotoBase64);

            var updatedOwner = await _unitOfWork.Owners.UpdateAsync(existingOwner);
            await _unitOfWork.CompleteAsync();
            await _unitOfWork.CommitTransactionAsync();

            var ownerDto = _mapper.Map<OwnerDto>(updatedOwner);

            _logger.LogInformation("Owner {OwnerId} updated", id);
            return ApiResponseDto<OwnerDto>.SuccessResponse(ownerDto, "Owner updated successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error updating owner with id {OwnerId}", id);
            return ApiResponseDto<OwnerDto>.ErrorResponse("Error updating owner");
        }
    }

    public async Task<ApiResponseDto<bool>> DeleteAsync(int id)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Use overridden DeleteAsync that checks for active properties
            var result = await _unitOfWork.Owners.DeleteAsync(id);
            if (!result)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponseDto<bool>.ErrorResponse("Owner not found");
            }

            await _unitOfWork.CompleteAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Owner {OwnerId} deleted", id);
            return ApiResponseDto<bool>.SuccessResponse(true, "Owner deleted successfully");
        }
        catch (InvalidOperationException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogWarning("Cannot delete owner {OwnerId}: {Message}", id, ex.Message);
            return ApiResponseDto<bool>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error deleting owner with id {OwnerId}", id);
            return ApiResponseDto<bool>.ErrorResponse("Error deleting owner");
        }
    }
}
