using Application.Dtos;
using Application.Interfaces.Services;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
[Authorize]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AssessmentsController(IAssessmentService assessmentService, IQuestionService questionService, ISubmissionService _submissionService) : ControllerBase
{

    // POST /api/assessments
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssessmentRequestModel model)
    {
        var response = await assessmentService.CreateAssessmentAsync(model);
        return response.Status ? Created("response", response) : BadRequest(response);
    }
    [HttpGet]
    public async Task<IActionResult> GetAllAssessments(
    [FromQuery] Guid? batchId,
    [FromQuery] DateTime? startDate,
    [FromQuery] DateTime? endDate,
    [FromQuery] string? search,
    [FromQuery] PaginationRequest request)
    {
        var result = await assessmentService.GetAllAssessmentsAsync(batchId, startDate, endDate, search, request);
        return Ok(result);
    }

    // GET /api/assessments/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await assessmentService.GetAssessmentAsync(id);
        return response.Status ? Ok(response) : NotFound(response);
    }
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAssessmentRequestModel model)
    {
        var response = await assessmentService.UpdateAssessmentAsync(id, model);
        return response.Status ? Ok(response) : BadRequest(response);
    }
    [HttpPost("{assessmentId:guid}/questions")]
    public async Task<IActionResult> AddQuestions(Guid assessmentId, [FromBody] List<CreateQuestionRequestModel> questions)
    {
        var response = await questionService.CreateQuestionsAsync(questions, assessmentId);
        return response ? Ok(response) : BadRequest(response);
    }
    [HttpGet("{assessmentId:guid}/questions/answers")]
    public async Task<IActionResult> GetQuestionsByAssessmentId(Guid assessmentId)
    {
        var response = await questionService.GetAllQuestionsByAssessmentIdAsync(assessmentId);
        return response.Status ? Ok(response) : NotFound(response);
    }
    [HttpGet("{assessmentId:guid}/questions")]
    public async Task<IActionResult> GetQuestionsForAssessment(Guid assessmentId)
    {
        var response = await questionService.GetAssessmentForAttemptAsync(assessmentId);
        return response.Status ? Ok(response) : NotFound(response);
    }

    [HttpPost("{assessmentId:guid}/submit")]
    public async Task<IActionResult> SubmitAssessment(Guid assessmentId, [FromBody] AnswerSubmissionDto model)
    {
        var response = await _submissionService.SubmitAssessment(assessmentId, model);
        return response.Status ? Ok(response) : BadRequest(response);
    }
    [HttpGet("{assessmentId:guid}/submissions")]
    public async Task<IActionResult> GetAssessmentSubmission(Guid assessmentId, [FromQuery] PaginationRequest request)
    {
        var response = await _submissionService.GetAssessmentSubmissions(assessmentId, request);
        return Ok(response);
    }

    [HttpGet("{assessmentId:guid}/student-answers")]    
    public async Task<IActionResult> GetMySubmission(Guid assessmentId)
    {
        var response = await _submissionService.GetCurrentStudentSubmission(assessmentId);
        return response.Status ? Ok(response) : NotFound(response);
    }
    [HttpGet("{assessmentId:guid}/student/submission")]
    public async Task<IActionResult> GetAssessmentSubmission(Guid assessmentId)
    {
        var response = await _submissionService.GetCurrentStudentSubmission(assessmentId);
        return response.Status ? Ok(response) : NotFound(response);
    }
    [HttpGet("{assessmentId:guid}/submissions/{studentId:guid}")]
    public async Task<IActionResult> GetSubmissionByStudent(Guid assessmentId, Guid studentId)
    {
        var response = await _submissionService.GetStudentSubmissionAsync(assessmentId, studentId);
        return response.Status ? Ok(response) : NotFound(response);
    }
    [HttpGet("recents")]
    public async Task<IActionResult> GetRecentAssessments()
    {
        var response = await assessmentService.GetRecentAssessment();
        return response.Status ? Ok(response) : NotFound(response);
    }

    [HttpGet("assessment-scores")]
    public async Task<IActionResult> GetInstructorAssessmentScores()
    {
        var result = await assessmentService.GetInstructorAssessmentScoresAsync();
        return Ok(result);
    }
    [HttpGet("{assessmentId:guid}/metrics")]
    public async Task<IActionResult> GetAssessmentMetrics(Guid assessmentId)
    {
        var result = await assessmentService.GetAssessmentMetrics(assessmentId);

        return Ok(result);
    }
    [HttpGet("{assessmentId:guid}/batch-performance")]
    public async Task<IActionResult> GetBatchPerformance(Guid assessmentId)
    {
        var result = await assessmentService.GetBatchPerformance(assessmentId); 
        return Ok(result);
    } 
    [HttpGet("{assessmentId:guid}/students")]
    public async Task<IActionResult> GetStudentAssessmentPerformance(Guid assessmentId, [FromQuery] PaginationRequest request)
    {
        var result = await assessmentService.GetStudentAssessmentPerformance(assessmentId,request); 
        return Ok(result);
    }
    [HttpGet("{assessmentId:guid}/score-distribution")]
    public async Task<IActionResult> GetScoreDistribution(Guid assessmentId)
    {
        var result = await assessmentService.GetAssessmentScoreDistribution(assessmentId); 
        return Ok(result);
    }
    

}

