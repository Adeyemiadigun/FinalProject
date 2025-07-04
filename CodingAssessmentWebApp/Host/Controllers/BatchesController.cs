﻿using Application.Dtos;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class BatchController : ControllerBase
    {
        private readonly IBatchService _batchService;
        private readonly IUserService _userService;
        private readonly IAssessmentService _assessmentService;

        public BatchController(IBatchService batchService, IUserService userService)
        {
            _batchService = batchService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBatch([FromBody] CreateBatchRequestModel request)
        {
            var result = await _batchService.CreateBatchAsync(request);
            return CreatedAtAction(nameof(GetBatchById), new { id = result.Data.Id }, result);
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
        [HttpGet]
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
        [HttpGet("/summary")]
        public async Task<IActionResult> GetBatchSummaries()
        {
            var result = await _batchService.GetBatchSummariesAsync();
            return Ok(result);
        }

    }
}
