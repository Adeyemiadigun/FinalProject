using Application.Dtos;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class QuestionsController(IQuestionService _questionService) : ControllerBase
    {

        [HttpPut("{questionId:guid}")]
        public async Task<IActionResult> UpdateQuestion(Guid questionId, UpdateQuestionDto model)
        {
            var response = await _questionService.UpdateQuestion(questionId, model);
            return response.Status ? Ok(response) : BadRequest(response);
        }
        [HttpDelete("{questionId:guid}")]
        public async Task<IActionResult> DeleteQuestion(Guid questionId)
        {
            var response = await _questionService.DeleteQuestionAsync(questionId);
            return response.Status ? Ok(response) : NotFound();
        }
    }
}
