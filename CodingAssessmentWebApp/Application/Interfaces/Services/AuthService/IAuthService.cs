using Application.Dtos;

namespace Application.Interfaces.Services.AuthService
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequestModel user);
        string GenerateJwtToken(UserDto userDto);
        string GenerateRefreshToken();
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task<BaseResponse<bool>> ForgetPassword(string Email);
        Task<BaseResponse<bool>> ResetPassword(ResetPasswordDto model);
    }
}
