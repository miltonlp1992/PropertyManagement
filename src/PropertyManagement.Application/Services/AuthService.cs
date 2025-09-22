using AutoMapper;
using Microsoft.Extensions.Logging;
using PropertyManagement.Application.DTOs;
using PropertyManagement.Application.DTOs.Auth;
using PropertyManagement.Application.Interfaces;
using PropertyManagement.Domain.Entities;
using PropertyManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginDto loginDto)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(loginDto.Username);

            if (user == null || !user.IsActive)
            {
                return ApiResponseDto<AuthResponseDto>.ErrorResponse("Invalid username or password");
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return ApiResponseDto<AuthResponseDto>.ErrorResponse("Invalid username or password");
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);

            // Generate tokens
            var accessToken = _jwtTokenService.GenerateAccessToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            // Save refresh token
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days
                IdUser = user.IdUser
            };

            await _unitOfWork.RefreshTokens.CreateAsync(refreshTokenEntity);
            await _unitOfWork.CompleteAsync();

            var authResponse = new AuthResponseDto
            {
                UserId = user.IdUser,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role ?? "User",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _logger.LogInformation("User {Username} logged in successfully", user.Username);
            return ApiResponseDto<AuthResponseDto>.SuccessResponse(authResponse, "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", loginDto.Username);
            return ApiResponseDto<AuthResponseDto>.ErrorResponse("Login failed");
        }
    }

    public async Task<ApiResponseDto<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            // Check if username already exists
            var existingUser = await _unitOfWork.Users.GetByUsernameAsync(registerDto.Username);
            if (existingUser != null)
            {
                return ApiResponseDto<AuthResponseDto>.ErrorResponse("Username already exists");
            }

            // Check if email already exists
            var existingEmail = await _unitOfWork.Users.GetByEmailAsync(registerDto.Email);
            if (existingEmail != null)
            {
                return ApiResponseDto<AuthResponseDto>.ErrorResponse("Email already exists");
            }

            // Create new user
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Role = registerDto.Role ?? "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.CreateAsync(user);
            await _unitOfWork.CompleteAsync();

            // Generate tokens
            var accessToken = _jwtTokenService.GenerateAccessToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            // Save refresh token
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IdUser = user.IdUser
            };

            await _unitOfWork.RefreshTokens.CreateAsync(refreshTokenEntity);
            await _unitOfWork.CompleteAsync();

            var authResponse = new AuthResponseDto
            {
                UserId = user.IdUser,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _logger.LogInformation("New user {Username} registered successfully", user.Username);
            return ApiResponseDto<AuthResponseDto>.SuccessResponse(authResponse, "Registration successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Username}", registerDto.Username);
            return ApiResponseDto<AuthResponseDto>.ErrorResponse("Registration failed");
        }
    }

    public async Task<ApiResponseDto<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        try
        {
            var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshTokenDto.RefreshToken);

            if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                return ApiResponseDto<AuthResponseDto>.ErrorResponse("Invalid or expired refresh token");
            }

            var user = await _unitOfWork.Users.GetByIdAsync(refreshToken.IdUser);
            if (user == null || !user.IsActive)
            {
                return ApiResponseDto<AuthResponseDto>.ErrorResponse("User not found or inactive");
            }

            // Revoke old refresh token
            refreshToken.IsRevoked = true;
            await _unitOfWork.RefreshTokens.UpdateAsync(refreshToken);

            // Generate new tokens
            var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            // Save new refresh token
            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IdUser = user.IdUser
            };

            await _unitOfWork.RefreshTokens.CreateAsync(newRefreshTokenEntity);
            await _unitOfWork.CompleteAsync();

            var authResponse = new AuthResponseDto
            {
                UserId = user.IdUser,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role ?? "User",
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            return ApiResponseDto<AuthResponseDto>.SuccessResponse(authResponse, "Token refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return ApiResponseDto<AuthResponseDto>.ErrorResponse("Token refresh failed");
        }
    }

    public async Task<ApiResponseDto<bool>> LogoutAsync(string refreshToken)
    {
        try
        {
            var token = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
            if (token != null)
            {
                token.IsRevoked = true;
                await _unitOfWork.RefreshTokens.UpdateAsync(token);
                await _unitOfWork.CompleteAsync();
            }

            return ApiResponseDto<bool>.SuccessResponse(true, "Logout successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return ApiResponseDto<bool>.ErrorResponse("Logout failed");
        }
    }

    public async Task<ApiResponseDto<bool>> RevokeUserTokensAsync(int userId)
    {
        try
        {
            await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(userId);
            await _unitOfWork.CompleteAsync();

            return ApiResponseDto<bool>.SuccessResponse(true, "All user tokens revoked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking user tokens for user {UserId}", userId);
            return ApiResponseDto<bool>.ErrorResponse("Failed to revoke user tokens");
        }
    }
}
