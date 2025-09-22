using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PropertyManagement.Application.DTOs;
using PropertyManagement.Application.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PropertyManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class PropertiesController : ControllerBase
{
    private readonly IPropertyService _propertyService;
    private readonly ILogger<PropertiesController> _logger;

    public PropertiesController(IPropertyService propertyService, ILogger<PropertiesController> logger)
    {
        _propertyService = propertyService;
        _logger = logger;
    }

    /// <summary>
    /// Get a property by ID
    /// </summary>
    /// <param name="id">Property ID</param>
    /// <returns>Property details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseDto<PropertyDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<PropertyDto>), 404)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<ApiResponseDto<PropertyDto>>> GetProperty(int id)
    {
        var result = await _propertyService.GetByIdAsync(id);

        if (!result.Success)
            return NotFound(result);

        // Log user access
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        _logger.LogInformation("User {Username} accessed property {PropertyId}", username, id);

        return Ok(result);
    }

    /// <summary>
    /// Get filtered list of properties with pagination
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <returns>Paginated list of properties</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<PagedResultDto<PropertyDto>>), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<PropertyDto>>>> GetProperties([FromQuery] PropertyFilterDto filter)
    {
        var result = await _propertyService.GetFilteredAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Create a new property (Admin only)
    /// </summary>
    /// <param name="createPropertyDto">Property creation data</param>
    /// <returns>Created property</returns>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponseDto<PropertyDto>), 201)]
    [ProducesResponseType(typeof(ApiResponseDto<PropertyDto>), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponseDto<PropertyDto>>> CreateProperty([FromBody] CreatePropertyDto createPropertyDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
            return BadRequest(ApiResponseDto<PropertyDto>.ErrorResponse("Validation failed", errors));
        }

        var result = await _propertyService.CreateAsync(createPropertyDto);

        if (!result.Success)
            return BadRequest(result);

        // Log creation
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        _logger.LogInformation("User {Username} created property {PropertyName}", username, createPropertyDto.Name);

        return CreatedAtAction(nameof(GetProperty), new { id = result.Data!.IdProperty }, result);
    }

    /// <summary>
    /// Update an existing property (Admin only)
    /// </summary>
    /// <param name="id">Property ID</param>
    /// <param name="updatePropertyDto">Property update data</param>
    /// <returns>Updated property</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponseDto<PropertyDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<PropertyDto>), 400)]
    [ProducesResponseType(typeof(ApiResponseDto<PropertyDto>), 404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponseDto<PropertyDto>>> UpdateProperty(int id, [FromBody] UpdatePropertyDto updatePropertyDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
            return BadRequest(ApiResponseDto<PropertyDto>.ErrorResponse("Validation failed", errors));
        }

        var result = await _propertyService.UpdateAsync(id, updatePropertyDto);

        if (!result.Success)
            return result.Message == "Property not found" ? NotFound(result) : BadRequest(result);

        // Log update
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        _logger.LogInformation("User {Username} updated property {PropertyId}", username, id);

        return Ok(result);
    }

    /// <summary>
    /// Change property price (Admin only)
    /// </summary>
    /// <param name="id">Property ID</param>
    /// <param name="newPrice">New price</param>
    /// <returns>Success status</returns>
    [HttpPatch("{id}/price")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), 400)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), 404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponseDto<bool>>> ChangePrice(int id, [FromBody][Required] decimal newPrice)
    {
        if (newPrice < 0)
            return BadRequest(ApiResponseDto<bool>.ErrorResponse("Price cannot be negative"));

        var result = await _propertyService.ChangePriceAsync(id, newPrice);

        if (!result.Success)
            return result.Message == "Property not found" ? NotFound(result) : BadRequest(result);

        // Log price change
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        _logger.LogInformation("User {Username} changed price for property {PropertyId} to {NewPrice}", username, id, newPrice);

        return Ok(result);
    }

    /// <summary>
    /// Delete a property (Admin only - soft delete)
    /// </summary>
    /// <param name="id">Property ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), 404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponseDto<bool>>> DeleteProperty(int id)
    {
        var result = await _propertyService.DeleteAsync(id);

        if (!result.Success)
            return NotFound(result);

        // Log deletion
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        _logger.LogInformation("User {Username} deleted property {PropertyId}", username, id);

        return Ok(result);
    }
}