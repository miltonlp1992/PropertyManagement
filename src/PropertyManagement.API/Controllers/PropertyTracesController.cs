using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PropertyManagement.Application.DTOs;
using PropertyManagement.Application.Interfaces;
using System.Security.Claims;

namespace PropertyManagement.API.Controllers;

[ApiController]
[Route("api/properties/{propertyId}/traces")]
[Authorize]
public class PropertyTracesController : ControllerBase
{
    private readonly IPropertyTraceService _propertyTraceService;
    private readonly ILogger<PropertyTracesController> _logger;

    public PropertyTracesController(IPropertyTraceService propertyTraceService, ILogger<PropertyTracesController> logger)
    {
        _propertyTraceService = propertyTraceService;
        _logger = logger;
    }

    /// <summary>
    /// Get all traces for a property
    /// </summary>
    /// <param name="propertyId">Property ID</param>
    /// <returns>List of property traces</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<PropertyTraceDto>>), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<PropertyTraceDto>>>> GetTraces(int propertyId)
    {
        var result = await _propertyTraceService.GetByPropertyIdAsync(propertyId);
        return Ok(result);
    }

    /// <summary>
    /// Create a new property trace (Admin only)
    /// </summary>
    /// <param name="propertyId">Property ID</param>
    /// <param name="createPropertyTraceDto">Property trace creation data</param>
    /// <returns>Created property trace</returns>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponseDto<PropertyTraceDto>), 201)]
    [ProducesResponseType(typeof(ApiResponseDto<PropertyTraceDto>), 400)]
    [ProducesResponseType(typeof(ApiResponseDto<PropertyTraceDto>), 404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponseDto<PropertyTraceDto>>> CreateTrace(int propertyId, [FromBody] CreatePropertyTraceDto createPropertyTraceDto)
    {
        // Ensure the propertyId in the route matches the DTO
        if (propertyId != createPropertyTraceDto.IdProperty)
        {
            return BadRequest(ApiResponseDto<PropertyTraceDto>.ErrorResponse("Property ID in route and body must match"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
            return BadRequest(ApiResponseDto<PropertyTraceDto>.ErrorResponse("Validation failed", errors));
        }

        var result = await _propertyTraceService.CreateAsync(createPropertyTraceDto);

        if (!result.Success)
        {
            return result.Message == "Property not found" ? NotFound(result) : BadRequest(result);
        }

        // Log trace creation
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        _logger.LogInformation("User {Username} created trace for property {PropertyId}", username, propertyId);

        return CreatedAtAction(nameof(GetTraces), new { propertyId }, result);
    }
}