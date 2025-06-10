using Application.Dtos;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
    {
        var response = await _userService.LoginAsync(model);
        return StatusCode(response.Status ? 200 : 400, response);
    }

    [HttpPost("register-instructor")]
    public async Task<IActionResult> RegisterInstructor([FromBody] RegisterUserRequestModel model)
    {
        var response = await _userService.RegisterInstructor(model);
        return StatusCode(response.Status ? 201 : 400, response);
    }

    [HttpPost("register-students")]
    public async Task<IActionResult> RegisterStudents([FromBody] BulkRegisterUserRequestModel model)
    {
        var response = await _userService.RegisterStudents(model);
        return StatusCode(response.Status ? 201 : 400, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var response = await _userService.GetAsync(id);
        return StatusCode(response.Status ? 200 : 404, response);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequsteModel model)
    {
        var response = await _userService.UpdateUser(model);
        return StatusCode(response.Status ? 200 : 400, response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var response = await _userService.DeleteAsync(id);
        return StatusCode(response.Status ? 200 : 404, response);
    }
}
