using Application.Dtos;
using Application.Interfaces.Services;
using Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class InstructorsController(IAssessmentService _assessmentService, IUserService _userService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> RegisterInstructor([FromBody] RegisterIstructorRequestModel model)
        {
            var response = await _userService.RegisterInstructor(model);
            return response.Status ? Created("response", response) : BadRequest(response);
        }
        [HttpGet]
        public async Task<IActionResult> GetInstructors()
        {
            var response = await _userService.GetInstructors();
            return Ok(response);
        }
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentInstructorProfile()
        {
            var response = await _userService.GetCurrentInstructorProfile();
            return Ok(response);
        }

        [HttpGet("{instructorId:guid}/assessments")]
        public async Task<IActionResult> GetAssessments(Guid instructorId, [FromQuery] PaginationRequest request)
        {
            var response = await _assessmentService.GetAllAssessmentsByInstructorIdAsync(request, instructorId);
            return response.Status ? Ok(response) : NotFound(response);
        }
        [HttpGet("assessments")]
        public async Task<IActionResult> GetInstructorAssessments([FromQuery] Guid? batchId,[FromQuery] AssessmentStatus? status,[FromQuery] PaginationRequest request)
        {
            var result = await _assessmentService.GetAssessmentsByInstructorAsync(batchId, status, request);
            return Ok(result);
        }
        [HttpGet("assessments/summaries")]
        public async Task<IActionResult> GetInstructorAssessments([FromQuery] PaginationRequest request)
        {
            var response = await _assessmentService.GetAllAssessmentsByInstructorIdAsync(request);
            return Ok(response);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchStudents([FromQuery] string? query, [FromQuery] string? status,[FromQuery]PaginationRequest request)
        {
            var results = await _userService.SearchInstructorByNameOrEmailAsync(query,request,status);
            return Ok(results);
        }
        [HttpGet("assessment/recents")]
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
        public async Task<IActionResult> GetInstructorAssessmentsDetails([FromRoute] Guid instructorId,[FromQuery] PaginationRequest request)
        {
            var result = await _assessmentService.GetInstructorAssessmentDetail(instructorId,request);
            return Ok(result);
        }
        [HttpGet("assessments/score-by-types")]
        public async Task<IActionResult> GetScoreByTypeForAllStudents(
        [FromQuery] Guid? batchId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
        {
            var result = await _userService.GetScoreByTypeForInstructorAsync(batchId, startDate, endDate);
            return Ok(result);
        }
    }

}
