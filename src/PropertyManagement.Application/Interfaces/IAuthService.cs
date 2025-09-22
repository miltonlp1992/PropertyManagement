using PropertyManagement.Application.DTOs;
using PropertyManagement.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginDto loginDto);
    Task<ApiResponseDto<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<ApiResponseDto<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task<ApiResponseDto<bool>> LogoutAsync(string refreshToken);
    Task<ApiResponseDto<bool>> RevokeUserTokensAsync(int userId);
}