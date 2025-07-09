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
    }
}
