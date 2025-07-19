using Application.Dtos;
using Application.Interfaces.Services.AuthService;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class AuthController(IAuthService _authService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
        {
            var response = await _authService.LoginAsync(model);
            return response.Status ? Ok(response) : BadRequest(response);
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            return result.Status ? Ok(result) : Unauthorized("Invalid or expired refresh token");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string Email)
        {
            var response = await _authService.ForgetPassword(Email);

            return response.Status? Ok(response) : Ok();
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ResetPasswordDto model)
        {
            var response = await _authService.ResetPassword(model);
            return response.Status? Ok(response) : Ok();
        }
    }
}
