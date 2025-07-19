using Application.Dtos;
using Application.Interfaces.Services;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Host.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class StudentsController(IUserService _userService, IAssessmentService _assementService,ISubmissionService submissionService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> RegisterStudents([FromBody] BulkRegisterUserRequestModel model)
        {
            var response = await _userService.RegisterStudents(model);
            return response.Status ? Created("Students Registered", response) : BadRequest(response);
        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadStudentFile(UploadFileDto fileUpload)
        {
            var res = await _userService.UploadStudentFileAsync(fileUpload);
            return Ok(res);
        }
        [HttpGet("{studentId:guid}/details")]
        public async Task<IActionResult> GetStudentDetais(Guid studentId)
        {
            var response = await _userService.GetStudentDetail(studentId);
            return response.Status ? Ok(response) : NotFound(response);
        }
        [HttpGet("details")]
        public async Task<IActionResult> GetCurrentStudentDetais()
        {
            var response = await _userService.GetStudentDetail();
            return response.Status ? Ok(response) : NotFound(response);
        }
        [HttpGet("{studentId:guid}/assessments")]
        public async Task<IActionResult> GetStudentsAssessmentById(Guid studentId, [FromQuery] PaginationRequest request)
        {
            var response = await _assementService.GetAssessmentsByStudentId(studentId, request);
            return response.Status ? Ok(response) : NotFound(response);
        }
        [HttpGet("assessments")]
        public async Task<IActionResult> GetStudentsAssessment([FromQuery] PaginationRequest request, [FromQuery] string? status)
        {
            var response = await _assementService.GetCurrentStudentAssessments(request, status);
            return response.Status ? Ok(response) : NotFound(response);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchStudents([FromQuery] Guid? batchId,[FromQuery] string? query, [FromQuery] string? status , [FromQuery] PaginationRequest request )
        {

            var results = await _userService.SearchByNameOrEmailAsync(batchId,query,request, status);
            return Ok(results);
        }

        [HttpGet("leaderboard")]
        public async Task<IActionResult> GetLeaderboard([FromQuery] Guid batchId, [FromQuery] PaginationRequest request)
        {
            var result = await _userService.GetLeaderboardAsync(batchId, request);
            return Ok(result);
        }
        [HttpGet("batch/leaderboard")]
        public async Task<IActionResult> CurrentStudentLeaaderBoard([FromQuery] PaginationRequest request)
        {
            var result = await _userService.GetStudentBatchLeaderboardAsync(request);
            return Ok(result);
        }
        [HttpGet("{studentId:guid}/analytics")]
        public async Task<IActionResult> GetStudentAnalytics(Guid studentId)
        {
            var result = await _userService.GetStudentAnalytics(studentId);
            return result.Status ? Ok(result) : NotFound(result);
        }
        [HttpPatch("{studentId:guid}/reassign-batch")]
        public async Task<IActionResult> UpdateStudentBatch(Guid studentId, Guid batchId)
        {
            var result = await _userService.UpdateStudentBatch(studentId, batchId);
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
        [HttpGet("submissions")]
        public async Task<IActionResult> GetStudentSubmissions([FromQuery] PaginationRequest request)
        {
                var result = await submissionService.GetCurrentStudentSubmission(request);
            return Ok(result);
        }
        [HttpGet("{studentId:guid}/submissions")]
        public async Task<IActionResult> GetStudentSubmissions(Guid studentId)
        {
            var result = await _userService.GetStudentSubmissionsAsync(studentId);
            return Ok(result);
        }
        [HttpGet("{submissionId:guid}/submission")]
        public async Task<IActionResult> GetSubmissionById(Guid submissionId)
        {
            var response = await submissionService.GetSubmissionByIdAsync(submissionId);
            return response.Status ? Ok(response) : NotFound(response);
        }

        [HttpGet("metrics/summary")]
        public async Task<IActionResult> GetStudentMetrics()
        {
            var result = await _userService.GetStudentMetrics();
            return Ok(result);
        }

    }
}
    