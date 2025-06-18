using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.AuthService;
using Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services.AuthService
{
    public class AuthService: IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IRefreshTokenStore _refreshTokenStore;

        public AuthService(IUserRepository userRepository, IOptions<JwtSettings> jwtOptions, ICurrentUser currentUser, IRefreshTokenStore refreshTokenStore)
        {
            _userRepository = userRepository;
            _jwtSettings = jwtOptions.Value;
            _currentUser = currentUser;
            _refreshTokenStore = refreshTokenStore;
        }

        public string GenerateJwtToken(UserDto userDto)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
                    new Claim(ClaimTypes.Email, userDto.Email),
                   new Claim(ClaimTypes.Role, userDto.Role.ToString())
                };
           

            var token = new JwtSecurityToken
            (
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.ExpiryTime),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public async Task<AuthResponse> LoginAsync(LoginRequestModel user)
        {
            var LoggedUser = await _userRepository.GetAsync(x => x.Email == user.Email);
            if (LoggedUser == null)
            {
                throw new ApiException("User not found", 404, "USER_NOT_FOUND", null);
            }
            if (!BCrypt.Net.BCrypt.Verify(user.Password, LoggedUser.PasswordHash))
            {
                throw new ApiException("Invalid password", 401, "INVALID_PASSWORD", null);
            }
            var userDto = new UserDto()
            {
                Id = LoggedUser.Id,
                Email = user.Email,
                Role = LoggedUser.Role,
            };
            var AccessToken = GenerateJwtToken(userDto);
            var RefreshToken = GenerateRefreshToken();

            await _refreshTokenStore.SaveTokenAsync(LoggedUser.Id, RefreshToken);
            return new AuthResponse
            {
                AccessToken = AccessToken,
                RefreshToken = RefreshToken,
                Status = true
            };

        }

        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            
            var response = await _refreshTokenStore.IsTokenValidAsync(refreshToken);
            if (!response.Isvalid)
            {
                throw new ApiException("Invalid refresh token", 401, "INVALID_REFRESH_TOKEN", null);
            }
            var user = await _userRepository.GetAsync(response.UserId);
            var userDto = new UserDto
            {
                Id = user!.Id,
                Email = user.Email,
                Role = user.Role
            };
            var newAccessToken = GenerateJwtToken(userDto);
            var newRefreshToken = GenerateRefreshToken();

            await _refreshTokenStore.SaveTokenAsync(user.Id, newRefreshToken);

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Status = true
            };
        }
    }
}
