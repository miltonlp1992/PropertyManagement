using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PropertyManagement.Application.DTOs;
using PropertyManagement.Application.Interfaces;
using PropertyManagement.Domain.Interfaces;
using System.Security.Claims;

namespace PropertyManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OwnersController : ControllerBase
{
    private readonly IOwnerService _ownerService;
    private readonly ILogger<OwnersController> _logger;

    public OwnersController(IOwnerService ownerService, ILogger<OwnersController> logger)
    {
        _ownerService = ownerService;
        _logger = logger;
    }

    /// <summary>
    /// Get all owners
    /// </summary>
    /// <returns>List of all owners</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<OwnerDto>>), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<OwnerDto>>>> GetOwners()
    {
        var result = await _ownerService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get an owner by ID
    /// </summary>
    /// <param name="id">Owner ID</param>
    /// <returns>Owner details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseDto<OwnerDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<OwnerDto>), 404)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<ApiResponseDto<OwnerDto>>> GetOwner(int id)
    {
        var result = await _ownerService.GetByIdAsync(id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Create a new owner (Admin only)
    /// </summary>
    /// <param name="createOwnerDto">Owner creation data</param>
    /// <returns>Created owner</returns>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponseDto<OwnerDto>), 201)]
    [ProducesResponseType(typeof(ApiResponseDto<OwnerDto>), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponseDto<OwnerDto>>> CreateOwner([FromBody] CreateOwnerDto createOwnerDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
            return BadRequest(ApiResponseDto<OwnerDto>.ErrorResponse("Validation failed", errors));
        }

        var result = await _ownerService.CreateAsync(createOwnerDto);

        if (!result.Success)
            return BadRequest(result);

        // Log owner creation
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        _logger.LogInformation("User {Username} created owner {OwnerName}", username, createOwnerDto.Name);

        return CreatedAtAction(nameof(GetOwner), new { id = result.Data!.IdOwner }, result);
    }

    /// <summary>
    /// Update an existing owner (Admin only)
    /// </summary>
    /// <param name="id">Owner ID</param>
    /// <param name="updateOwnerDto">Owner update data</param>
    /// <returns>Updated owner</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponseDto<OwnerDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<OwnerDto>), 400)]
    [ProducesResponseType(typeof(ApiResponseDto<OwnerDto>), 404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponseDto<OwnerDto>>> UpdateOwner(int id, [FromBody] UpdateOwnerDto updateOwnerDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
            return BadRequest(ApiResponseDto<OwnerDto>.ErrorResponse("Validation failed", errors));
        }

        var result = await _ownerService.UpdateAsync(id, updateOwnerDto);

        if (!result.Success)
            return result.Message == "Owner not found" ? NotFound(result) : BadRequest(result);

        // Log owner update
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        _logger.LogInformation("User {Username} updated owner {OwnerId}", username, id);

        return Ok(result);
    }

    /// <summary>
    /// Delete an owner (Admin only)
    /// </summary>
    /// <param name="id">Owner ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), 400)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), 404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponseDto<bool>>> DeleteOwner(int id)
    {
        try
        {
            var result = await _ownerService.DeleteAsync(id);

            if (!result.Success)
                return NotFound(result);

            // Log owner deletion
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            _logger.LogInformation("User {Username} deleted owner {OwnerId}", username, id);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<bool>.ErrorResponse(ex.Message));
        }
    }
}