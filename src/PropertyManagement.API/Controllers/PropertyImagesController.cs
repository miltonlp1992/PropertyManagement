using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PropertyManagement.Application.DTOs;
using PropertyManagement.Application.Interfaces;
using System.Security.Claims;

namespace PropertyManagement.API.Controllers;

[ApiController]
[Route("api/properties/{propertyId}/images")]
[Authorize]
public class PropertyImagesController : ControllerBase
{
    private readonly IPropertyImageService _propertyImageService;
    private readonly ILogger<PropertyImagesController> _logger;

    public PropertyImagesController(IPropertyImageService propertyImageService, ILogger<PropertyImagesController> logger)
    {
        _propertyImageService = propertyImageService;
        _logger = logger;
    }

    /// <summary>
    /// Add an image to a property (Admin only)
    /// </summary>
    /// <param name="propertyId">Property ID</param>
    /// <param name="file">Image file</param>
    /// <returns>Added image details</returns>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponseDto<PropertyImageDto>), 201)]
    [ProducesResponseType(typeof(ApiResponseDto<PropertyImageDto>), 400)]
    [ProducesResponseType(typeof(ApiResponseDto<PropertyImageDto>), 404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponseDto<PropertyImageDto>>> AddImage(int propertyId, IFormFile file)
    {
        var result = await _propertyImageService.AddImageAsync(propertyId, file);

        if (!result.Success)
        {
            return result.Message == "Property not found" ? NotFound(result) : BadRequest(result);
        }

        // Log image addition
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        _logger.LogInformation("User {Username} added image to property {PropertyId}", username, propertyId);

        return CreatedAtAction(nameof(GetImages), new { propertyId }, result);
    }

    /// <summary>
    /// Get all images for a property
    /// </summary>
    /// <param name="propertyId">Property ID</param>
    /// <returns>List of property images</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<PropertyImageDto>>), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<PropertyImageDto>>>> GetImages(int propertyId)
    {
        var result = await _propertyImageService.GetImagesByPropertyIdAsync(propertyId);
        return Ok(result);
    }

    /// <summary>
    /// Get image file by ID
    /// </summary>
    /// <param name="propertyId">Property ID</param>
    /// <param name="imageId">Image ID</param>
    /// <returns>Image file</returns>
    [HttpGet("{imageId}/file")]
    [ProducesResponseType(typeof(FileResult), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<ActionResult> GetImageFile(int propertyId, int imageId)
    {
        var result = await _propertyImageService.GetImageFileAsync(imageId);

        if (!result.Success || result.Data == null)
            return NotFound();

        return File(result.Data, "image/jpeg");
    }

    /// <summary>
    /// Delete an image (Admin only)
    /// </summary>
    /// <param name="propertyId">Property ID</param>
    /// <param name="imageId">Image ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{imageId}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), 404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponseDto<bool>>> DeleteImage(int propertyId, int imageId)
    {
        var result = await _propertyImageService.DeleteImageAsync(imageId);

        if (!result.Success)
            return NotFound(result);

        // Log image deletion
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        _logger.LogInformation("User {Username} deleted image {ImageId} from property {PropertyId}", username, imageId, propertyId);

        return Ok(result);
    }
}
