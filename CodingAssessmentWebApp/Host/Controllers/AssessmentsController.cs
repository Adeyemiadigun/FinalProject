using Application.Dtos;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AssessmentsController : ControllerBase
{
    private readonly IAssessmentService _assessmentService;

    public AssessmentsController(IAssessmentService assessmentService)
    {
        _assessmentService = assessmentService;
    }

    // POST /api/assessments
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssessmentRequestModel model)
    {
        var response = await _assessmentService.CreateAssessmentAsync(model);
        return StatusCode(response.Status ? 201 : 400, response);
    }

    // GET /api/assessments
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var response = await _assessmentService.GetAllAssessmentsAsync(request);
        return StatusCode(response.Status ? 200 : 400, response);
    }

    // GET /api/assessments/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _assessmentService.GetAssessmentAsync(id);
        return StatusCode(response.Status ? 200 : 404, response);
    }

    // GET /api/assessments/by-course/{courseId}
    [HttpGet("by-course/{courseId:guid}")]
    public async Task<IActionResult> GetByCourseId(Guid courseId)
    {
        var response = await _assessmentService.GetAllAssessmentsByCourseIdAsync(courseId);
        return StatusCode(response.Status ? 200 : 404, response);
    }

    // GET /api/assessments/by-instructor/{instructorId}
    [HttpGet("by-instructor/{instructorId:guid}")]
    public async Task<IActionResult> GetByInstructorId(Guid instructorId, [FromQuery] PaginationRequest request)
    {
        var response = await _assessmentService.GetAllAssessmentsByInstructorIdAsync(instructorId, request);
        return StatusCode(response.Status ? 200 : 404, response);
    }

    // POST /api/assessments/assign-students
    [HttpPost("assign-students")]
    public async Task<IActionResult> AssignStudents([FromBody] AssignStudentsModel model)
    {
        var response = await _assessmentService.AssignStudents(model);
        return StatusCode(response.Status ? 200 : 400, response);
    }
}

