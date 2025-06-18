using Application.Dtos;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class AiController(IAIQuestionService _aiProvider) : ControllerBase
    {
        [HttpPost("generate-text")]
        public async Task<IActionResult> GenerateTextAsync([FromBody] AiQuestionGenerationRequestDto request)
        {
            var response = await _aiProvider.GenerateQuestionAsync(request);

            return response.Status
                ? Ok(response)
                : BadRequest(response);
        }
    }
}
