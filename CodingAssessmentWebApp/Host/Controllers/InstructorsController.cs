using Application.Dtos;
using Application.Interfaces.Services;
using Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers
{
    [ApiController]
    [Route("api//v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class InstructorController(IAssessmentService _assessmentService, IUserService _userService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> RegisterInstructor([FromBody] RegisterUserRequestModel model)
        {
            var response = await _userService.RegisterInstructor(model);
            return response.Status ? Created("response", response) : BadRequest(response);
        }

        [HttpGet("{instructorId:guid}/assessments")]
        public async Task<IActionResult> GetAssessments(Guid instructorId, [FromQuery] PaginationRequest request)
        {
            var response = await _assessmentService.GetAllAssessmentsByInstructorIdAsync(request, instructorId);
            return response.Status ? Ok(response) : NotFound(response);
        }
    }

}
