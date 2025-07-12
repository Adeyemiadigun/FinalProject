using Application.Dtos;
using Application.Interfaces.Services;
using Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class InstructorController(IAssessmentService _assessmentService, IUserService _userService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> RegisterInstructor([FromBody] RegisterIstructorRequestModel model)
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
        [HttpGet("assessments")]
        public async Task<IActionResult> GetInstructorAssessments([FromQuery] Guid? batchId,[FromQuery] string? status,[FromQuery] PaginationRequest request)
        {
            var result = await _assessmentService.GetAssessmentsByInstructorAsync(batchId, status, request);
         
            return Ok(result);
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchStudents([FromQuery] string query,[FromQuery] string status)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Search term is required." });

            var results = await _userService.SearchInstructorByNameOrEmailAsync(query,status);
            return Ok(new { status = true, data = results });
        }
        [HttpGet("/assessment/recents")]
        public async Task<IActionResult> GetInstructorRecentAssessments()
        {
            var response = await _assessmentService.GetInstructorRecentAssessment();
            return response.Status ? Ok(response) : NotFound(response);
        }
        [HttpGet("{instructorId}/details")]
        public async Task<IActionResult> GetInstructorDetaisl(Guid instructorId)
        {
            var result = await _userService.GetInstructorDetails(instructorId);
            return Ok(result);
        }
        [HttpGet("{instructorId}/assessment/details")]
        public async Task<IActionResult> GetInstructorAssessmentsDetails([FromQuery] Guid instructorId,PaginationRequest request)
        {
            var result = await _assessmentService.GetInstructorAssessmentDetail(instructorId,request);
            return Ok(result);
        }
    }

}
