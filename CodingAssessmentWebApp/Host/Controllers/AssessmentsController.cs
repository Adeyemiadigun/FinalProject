using Application.Dtos;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AssessmentsController(IAssessmentService assessmentService, IQuestionService questionService) : ControllerBase
{

    // POST /api/assessments
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssessmentRequestModel model)
    {
        var response = await assessmentService.CreateAssessmentAsync(model);
        return response.Status ? Created("response", response) : BadRequest(response);
    }

    // GET /api/assessments
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var response = await assessmentService.GetAllAssessmentsAsync(request);
        return response.Status ? Ok(response) : BadRequest(response);
    }

    // GET /api/assessments/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await assessmentService.GetAssessmentAsync(id);
        return response.Status ? Ok(response) : NotFound(response);
    }

    // POST /api/assessments/assign-students
    [HttpPost("{id:guid}/students")]
    public async Task<IActionResult> AssignStudents(Guid id,[FromBody] AssignStudentsModel model)
    {
        var response = await assessmentService.AssignStudents(id,model);
        return response.Status ? Ok(response) : BadRequest(response);
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
    [HttpGet("{assessmentId:guid}/questions")]
    public async Task<IActionResult> GetQuestionsByAssessmentId(Guid assessmentId, [FromQuery] PaginationRequest request)
    {
        var response = await questionService.GetAllQuestionsByAssessmentIdAsync(assessmentId, request);
        return response.Status ? Ok(response) : NotFound(response);
    }


}

