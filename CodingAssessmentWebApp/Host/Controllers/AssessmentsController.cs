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
        return response.Status ? Created("response", response) : BadRequest(response);
    }

    // GET /api/assessments
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var response = await _assessmentService.GetAllAssessmentsAsync(request);
        return response.Status ? Ok(response) : BadRequest(response);
    }

    // GET /api/assessments/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _assessmentService.GetAssessmentAsync(id);
        return response.Status ? Ok(response) : NotFound(response);
    }

    // POST /api/assessments/assign-students
    [HttpPost("{id:guid}/students")]
    public async Task<IActionResult> AssignStudents(Guid id,[FromBody] AssignStudentsModel model)
    {
        var response = await _assessmentService.AssignStudents(id,model);
        return response.Status ? Ok(response) : BadRequest(response);
    }
}

