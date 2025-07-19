using Application.Dtos;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class BatchesController : ControllerBase
    {
        private readonly IBatchService _batchService;
        private readonly IUserService _userService;
        private readonly IAssessmentService _assessmentService;

        public BatchesController(IBatchService batchService, IUserService userService, IAssessmentService assessmentService)
        {
            _batchService = batchService;
            _userService = userService;
            _assessmentService = assessmentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBatch([FromBody] CreateBatchRequestModel request)
        {
            var result = await _batchService.CreateBatchAsync(request);
            return Created("Batch Created Successfully",result.Data);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetBatchById(Guid id)
        {
            var result = await _batchService.GetBatchByIdAsync(id);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBatches([FromQuery] PaginationRequest pagination)
        {
            var result = await _batchService.GetAllBatchesAsync(pagination);
            return Ok(result);
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllBatches()
        {
            var result = await _batchService.GetAllBatchesAsync();
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateBatch(Guid id, [FromBody] UpdateBatchRequestModel model)
        {
            var result = await _batchService.UpdateBatchAsync(id, model);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteBatch(Guid id)
        {
            var result = await _batchService.DeleteBatchAsync(id);
            return Ok(result);
        }

        [HttpPost("{batchId:guid}/assign-assessment/{assessmentId:guid}")]
        public async Task<IActionResult> AssignAssessmentToBatch(Guid batchId, Guid assessmentId)
        {
            var result = await _batchService.AssignAssessmentToBatchAsync(batchId, assessmentId);
            return Ok(result);
        }
        [HttpGet("{id:guid}/students")]
        public async Task<IActionResult> GetAllStudentsByBatchId(Guid id, [FromQuery] PaginationRequest request)
        {
            var result = await _userService.GetAllByBatchId(id, request);
            return Ok(result);
        }
        [HttpGet("{id:guid}/assessments")]
        public async Task<IActionResult> GetAssessmentById(Guid id, [FromQuery] PaginationRequest request)
        {
            var result = await _assessmentService.GetAssessmentsByBatchId(id, request);

            return Ok(result);
        } 
        [HttpGet("{id:guid}/assessments/details")]
        public async Task<IActionResult> GetAssessmentsByBatchIdDetails(Guid id, [FromQuery] PaginationRequest request)
        {
            var result = await _assessmentService.GetAssessmentsByBatchIdDetails(id, request);

            return Ok(result);
        }
        [HttpGet("summary")]
        public async Task<IActionResult> GetBatchSummaries()
        {
            var result = await _batchService.GetBatchSummariesAsync();
            return Ok(result);
        }
        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetBatchDetails(Guid id)
        {
            var result = await _batchService.GetBatchDetails(id);
            return Ok(result);

        }
        [HttpGet("{batchId}/performance-trend")]
        public async Task<IActionResult> GetBatchPerformanceTrend(Guid batchId)
        {
            var result = await _batchService.GetBatchPerformanceTrend(batchId);
            return Ok(result);
        }
        [HttpGet("students")]
        public async Task<IActionResult> GetStudents([FromQuery] Guid? batchId, [FromQuery] PaginationRequest request)
        {
            var result = await _userService.GetAllByBatchId(batchId, request);
            return Ok(result);
        }
        [HttpGet("{batchId:guid}/submission-stats")]
        public async Task<IActionResult> GetBatchSubmissionStats([FromRoute] Guid batchId)
        {
            var result = await _batchService.GetBatchSubmissionStats(batchId);
            return Ok(result);
        }

    }
}
