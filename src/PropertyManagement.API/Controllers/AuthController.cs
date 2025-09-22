using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PropertyManagement.Application.DTOs;
using PropertyManagement.Application.DTOs.Auth;
using PropertyManagement.Application.Interfaces;
using System.Security.Claims;

namespace PropertyManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// User login
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>Authentication response with tokens</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), 400)]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), 401)]
    public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
            return BadRequest(ApiResponseDto<AuthResponseDto>.ErrorResponse("Validation failed", errors));
        }

        var result = await _authService.LoginAsync(loginDto);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    /// <summary>
    /// User registration
    /// </summary>
    /// <param name="registerDto">Registration data</param>
    /// <returns>Authentication response with tokens</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), 400)]
    public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
            return BadRequest(ApiResponseDto<AuthResponseDto>.ErrorResponse("Validation failed", errors));
        }

        var result = await _authService.RegisterAsync(registerDto);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(Login), result);
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    /// <param name="refreshTokenDto">Refresh token</param>
    /// <returns>New authentication tokens</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), 400)]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), 401)]
    public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
            return BadRequest(ApiResponseDto<AuthResponseDto>.ErrorResponse("Validation failed", errors));
        }

        var result = await _authService.RefreshTokenAsync(refreshTokenDto);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    /// <summary>
    /// User logout
    /// </summary>
    /// <param name="refreshTokenDto">Refresh token to revoke</param>
    /// <returns>Logout status</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), 200)]
    public async Task<ActionResult<ApiResponseDto<bool>>> Logout([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var result = await _authService.LogoutAsync(refreshTokenDto.RefreshToken);
        return Ok(result);
    }

    /// <summary>
    /// Revoke all user tokens (useful for security purposes)
    /// </summary>
    /// <returns>Revoke status</returns>
    [HttpPost("revoke-all")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), 200)]
    public async Task<ActionResult<ApiResponseDto<bool>>> RevokeAllTokens()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _authService.RevokeUserTokensAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    /// <returns>Current user data</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
    public ActionResult<ApiResponseDto<object>> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var firstName = User.FindFirst("firstName")?.Value;
        var lastName = User.FindFirst("lastName")?.Value;

        var userData = new
        {
            UserId = userId,
            Username = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = role
        };

        return Ok(ApiResponseDto<object>.SuccessResponse(userData));
    }
}