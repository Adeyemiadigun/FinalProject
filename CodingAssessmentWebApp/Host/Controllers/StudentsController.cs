using Application.Dtos;
using Application.Interfaces.Services;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Host.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class StudentsController(IUserService _userService, IAssessmentService _assementService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> RegisterStudents([FromBody] BulkRegisterUserRequestModel model)
        {
            var response = await _userService.RegisterStudents(model);
            return response.Status ? Created("Students Registered", response) : BadRequest(response);
        }
        [HttpGet("{studentId:guid}/details")]
        public async Task<IActionResult> GetStudentDetais(Guid id)
        {
            var response = await _userService.GetStudentDetail(id);
            return response.Status ? Ok(response) : NotFound(response);
        }
        [HttpGet("{studentId:guid}/assessments")]
        public async Task<IActionResult> GetStudentsAssessmentById(Guid studentId, [FromQuery] PaginationRequest request)
        {
            var response = await _assementService.GetAssessmentsByStudentId(studentId, request);
            return response.Status ? Ok(response) : NotFound(response);
        }
        [HttpGet("assessments")]
        public async Task<IActionResult> GetStudentsAssessment([FromQuery] PaginationRequest request, [FromQuery] string status)
        {
            var response = await _assementService.GetCurrentStudentAssessments(request, status);
            return response.Status ? Ok(response) : NotFound(response);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchStudents([FromQuery] string query, [FromQuery] string status , [FromQuery] PaginationRequest request )
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Search term is required." });

            var results = await _userService.SearchByNameOrEmailAsync(query,request, status);
            return Ok(new { status = true, data = results });
        }

        [HttpGet("leaderboard")]
        public async Task<IActionResult> GetLeaderboard([FromQuery] PaginationRequest request, [FromQuery] Guid? batchId = null)
        {
            var result = await _userService.GetLeaderboardAsync(batchId, request);
            return Ok(result);
        }
        [HttpGet("{studentId:guid}/analytics")]
        public async Task<IActionResult> GetStudentAnalytics([FromQuery] Guid studentId)
        {
            var result = await _userService.GetStudentAnalytics(studentId);
            return result.Status ? Ok(result) : NotFound(result);
        }
        [HttpPost("{studentId:guid}/reassign-batch")]
        public async Task<IActionResult> UpdateStudentBatch(Guid studentId, ReAssignBatch batchId)
        {
            var result = await _userService.UpdateStudentBatch(studentId, batchId.BatchId);
            return result.Status ? Ok(result) : NotFound(result);
        }
        [HttpPatch("{studentId}/status")]
        public async Task<IActionResult> UpdateStatus(Guid studentId, [FromBody] UpdateStudentStatusDto dto)
        {
            var result = await _userService.UpdateStudentStatusAsync(studentId, dto.Status);
            return Ok(result);
        }
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _userService.GetSummaryAsync();
            return Ok(result);
        }

        [HttpGet("ongoing")]
        public async Task<IActionResult> GetOngoingAssessments()
        {
            var result = await _userService.GetOngoingAssessmentsAsync();
            return Ok(result);
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingAssessments()
        {
            var result = await _userService.GetUpcomingAssessmentsAsync();
            return Ok(result);
        }

        [HttpGet("performance-trend")]
        public async Task<IActionResult> GetPerformanceTrend()
        {
            var result = await _userService.GetStudentScoreTrendsAsync();
            return Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetSubmittedAssessments()
        {
            var result = await _userService.GetSubmittedAssessmentsAsync();
            return Ok(result);
        }

    }
}
