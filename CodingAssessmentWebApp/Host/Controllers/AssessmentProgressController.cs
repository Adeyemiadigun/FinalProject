using Application.Dtos;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;



namespace Host.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api:Version}[controller]")]
    [ApiController]
    public class AssessmentProgressController : ControllerBase
    {
        private readonly IStudentProgressService _progressService; 
        public AssessmentProgressController(IStudentProgressService progressService)
        {
            _progressService = progressService;
        }

        [HttpPost("students/progress/save")]
        public async Task<IActionResult> SaveProgress([FromBody] SaveProgressDto dto)
        {
            await _progressService.SaveProgressAsync(dto);
            return Ok(new { message = "Progress saved." });
        }
        [HttpGet("load")]
        public async Task<IActionResult> LoadProgress([FromQuery] Guid assessmentId)
        {

            var result = await _progressService.GetProgressAsync(assessmentId);
            if (!result.Status)
                return NotFound(result);

            return Ok(result);
        }
    }
}
