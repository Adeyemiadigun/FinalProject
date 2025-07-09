using Application.Dtos;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Host.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}[controller]")]
    [ApiVersion("1.0")]
    public class StudentsController(IUserService _userService, IAssessmentService _assementService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> RegisterStudents([FromBody] BulkRegisterUserRequestModel model)
        {
            var response = await _userService.RegisterStudents(model);
            return response.Status ? Created("Students Registered", response) : BadRequest(response);
        }
        [HttpGet("{studentId:guid}/assessments")]
        public async Task<IActionResult> GetStudentsAssessment(Guid studentId, PaginationRequest request)
        {
            var response = await _assementService.GetAssessmentsByStudentId( studentId, request);
            return response.Status ? Ok(response) : NotFound(response);
        }
        [HttpGet("assessments")]
        public async Task<IActionResult> GetStudentsAssessment( PaginationRequest request)
        {
            var response = await _assementService.GetCurrentStudentAssessments(request);
            return response.Status ? Ok(response) : NotFound(response);
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchStudents([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Search term is required." });

            var results = await _userService.SearchByNameOrEmailAsync(query);
            return Ok(new { status = true, data = results });
        }
    }
}
