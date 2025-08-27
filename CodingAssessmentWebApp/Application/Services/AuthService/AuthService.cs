using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.ExternalServices;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.AuthService;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Web;
using Application.Services.AuthService.Configurations;

namespace Application.Services.AuthService
{
    public class AuthService: IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IRefreshTokenStore _refreshTokenStore;
        private readonly IPasswordResetService _passwordResetService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly string resetLink;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUserRepository userRepository, IOptions<JwtSettings> jwtOptions, ICurrentUser currentUser, IRefreshTokenStore refreshTokenStore, IPasswordResetService resetService, IEmailService emailService, IConfiguration config, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _jwtSettings = jwtOptions.Value;
            _currentUser = currentUser;
            _refreshTokenStore = refreshTokenStore;
            _passwordResetService = resetService;
            _emailService = emailService;
            _config = config;
            _unitOfWork = unitOfWork;
            resetLink = _config["ClientUrl"];
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
                throw new ApiException("Invalid Credentials", 404, "INVALID_CREDENTIALS", null);
            }
            if (!BCrypt.Net.BCrypt.Verify(user.Password, LoggedUser.PasswordHash))
            {
                throw new ApiException("Invalid Credentials", 401, "INVALID_PASSWORD", null);
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
            await _refreshTokenStore.RemoveToken(refreshToken);
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
        public async Task<BaseResponse<bool>> ForgetPassword(string Email)
        {
            var user = await _userRepository.GetAsync(x => x.Email == Email);
                if (user is null)
                return new BaseResponse<bool> { Status = false };

            var token = _passwordResetService.GenerateResetToken(Email);
            var userDto = new UserDto
            {
                Email = Email,
                FullName = user.FullName
            };
            var encodedToken = HttpUtility.UrlEncode(token);
            var link = $"{resetLink}?email={Email}&token={encodedToken}";
            var body = TemplateBody(link);
           await  _emailService.SendEmailAsync(userDto, "Reset Link", body);

            return new BaseResponse<bool>
            {
                Message = "Check Email For Reset Link",
                Status = true
            };
        }
        public async Task<BaseResponse<bool>> ResetPassword(ResetPasswordDto model)
        {
            if (!_passwordResetService.ValidateToken(model.Email, model.Token))
                return new BaseResponse<bool> { Status = false, Message = "Invalid or expired token" };

            var user = await _userRepository.GetAsync(u => u.Email == model.Email);
            if (user == null)
                return new BaseResponse<bool> { Status = false, Message = "User not found" };

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
             _userRepository.Update(user);

            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse<bool> { Status = true, Message = "Password reset successful" };
        }

        private string TemplateBody(string resetLink)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                  <body style='font-family: Arial, sans-serif; color: #333;'>
                    <h2>Password Reset Request</h2>
                    <p>Hello,</p>
                    <p>
                      We received a request to reset the password for your account associated with this email.
                    </p>
                    <p>
                      Click the button below to reset your password. This link will expire in 15 minutes.
                    </p>
                    <p>
                      <a href='{resetLink}'
                         style='display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 4px;'>
                         Reset Password
                      </a>
                    </p>
                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                    <p style='word-break: break-word;'>{resetLink}</p>
                    <p>If you did not request this, please ignore this email.</p>
                    <p>Thank you,<br>YourApp Team</p>
                  </body>
                </html>";
        }

    }
}
