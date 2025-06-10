using Application.Dtos;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/instructors")]
    public class InstructorController(IAssessmentService _assessmentService) : ControllerBase
    {
        [HttpGet("{instructorId:guid}/assessments")]
        public async Task<IActionResult> GetAssessments(Guid instructorId, [FromQuery] PaginationRequest request)
        {
            var response = await _assessmentService.GetAllAssessmentsByInstructorIdAsync(request, instructorId);
            return response.Status ? Ok(response) : NotFound(response);
        }
    }

}
