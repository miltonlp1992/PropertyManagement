using AutoMapper;
using Microsoft.AspNetCore.Http;
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

public class PropertyImageService : IPropertyImageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PropertyImageService> _logger;

    public PropertyImageService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PropertyImageService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponseDto<PropertyImageDto>> AddImageAsync(int propertyId, IFormFile file)
    {
        try
        {
            // Validate property exists using generic method
            if (!await _unitOfWork.Properties.ExistsAsync(propertyId))
            {
                return ApiResponseDto<PropertyImageDto>.ErrorResponse("Property not found");
            }

            // Validate file
            if (file == null || file.Length == 0)
            {
                return ApiResponseDto<PropertyImageDto>.ErrorResponse("File is required");
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                return ApiResponseDto<PropertyImageDto>.ErrorResponse("Invalid file type. Only JPEG, PNG, and GIF are allowed");
            }

            // Validate file size (10MB max)
            if (file.Length > 10 * 1024 * 1024)
            {
                return ApiResponseDto<PropertyImageDto>.ErrorResponse("File size cannot exceed 10MB");
            }

            // Convert file to byte array
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            await _unitOfWork.BeginTransactionAsync();

            var propertyImage = new PropertyImage
            {
                IdProperty = propertyId,
                File = fileBytes,
                Enabled = true
            };

            var createdImage = await _unitOfWork.PropertyImages.CreateAsync(propertyImage);
            await _unitOfWork.CompleteAsync();
            await _unitOfWork.CommitTransactionAsync();

            var imageDto = _mapper.Map<PropertyImageDto>(createdImage);

            _logger.LogInformation("Image added to property {PropertyId}", propertyId);
            return ApiResponseDto<PropertyImageDto>.SuccessResponse(imageDto, "Image added successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error adding image to property {PropertyId}", propertyId);
            return ApiResponseDto<PropertyImageDto>.ErrorResponse("Error adding image");
        }
    }

    public async Task<ApiResponseDto<IEnumerable<PropertyImageDto>>> GetImagesByPropertyIdAsync(int propertyId)
    {
        try
        {
            // Use specific method from IPropertyImageRepository
            var images = await _unitOfWork.PropertyImages.GetEnabledByPropertyIdAsync(propertyId);
            var imageDtos = _mapper.Map<IEnumerable<PropertyImageDto>>(images);

            return ApiResponseDto<IEnumerable<PropertyImageDto>>.SuccessResponse(imageDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting images for property {PropertyId}", propertyId);
            return ApiResponseDto<IEnumerable<PropertyImageDto>>.ErrorResponse("Error retrieving images");
        }
    }

    public async Task<ApiResponseDto<bool>> DeleteImageAsync(int imageId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Use generic delete method (will call soft delete override)
            var result = await _unitOfWork.PropertyImages.DeleteAsync(imageId);
            if (!result)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponseDto<bool>.ErrorResponse("Image not found");
            }

            await _unitOfWork.CompleteAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Image {ImageId} deleted", imageId);
            return ApiResponseDto<bool>.SuccessResponse(true, "Image deleted successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error deleting image {ImageId}", imageId);
            return ApiResponseDto<bool>.ErrorResponse("Error deleting image");
        }
    }

    public async Task<ApiResponseDto<byte[]>> GetImageFileAsync(int imageId)
    {
        try
        {
            // Use generic GetByIdAsync method
            var image = await _unitOfWork.PropertyImages.GetByIdAsync(imageId);
            if (image == null || image.File == null || !image.Enabled)
            {
                return ApiResponseDto<byte[]>.ErrorResponse("Image not found");
            }

            return ApiResponseDto<byte[]>.SuccessResponse(image.File);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting image file {ImageId}", imageId);
            return ApiResponseDto<byte[]>.ErrorResponse("Error retrieving image file");
        }
    }
}
