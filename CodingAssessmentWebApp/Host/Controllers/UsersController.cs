using Application.Dtos;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var response = await _userService.GetAsync(id);
        return response.Status ? Ok(response) : NotFound(response);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequsteModel model)
    {
        var response = await _userService.UpdateUser(model);
        return response.Status ? Ok(response) : BadRequest(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var response = await _userService.DeleteAsync(id);
        return response.Status ? Ok(response) : NotFound(response);
    }
   
}
