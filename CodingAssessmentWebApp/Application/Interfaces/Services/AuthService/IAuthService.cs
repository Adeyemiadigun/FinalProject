﻿using Application.Dtos;

namespace Application.Interfaces.Services.AuthService
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequestModel user);
        string GenerateJwtToken(UserDto userDto);
        string GenerateRefreshToken();
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    }
}
