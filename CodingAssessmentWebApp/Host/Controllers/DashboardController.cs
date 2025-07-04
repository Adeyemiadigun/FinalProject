﻿using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;


namespace Host.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class DashboardController(IDashboardService _dashBoardService, IBatchService _batchService, IAssessmentService assessmentService) : ControllerBase
    {
        [HttpGet("admin/metrics/overview")]
        public async Task<IActionResult> GetOverview()
        {
            var result = await _dashBoardService.AdminDashBoardOverview();
            return Ok(result);
        }
        [HttpGet("admin/batches/analytics")]
        public async Task<IActionResult> GetBatchAnakytics()
        {
            var result = await _batchService.BatchAnalytics();
            return Ok(result);
        }
        //Endpoint: GET /api/v1/admin/analytics/assessments/top-performing
        [HttpGet("admin/analytics/assessments/top-performing")]
        public async Task<IActionResult> GetTopPerformingAssessments()
        {
            var result = await assessmentService.GetTopAssessments();
            return Ok(result);
        }
        //GET /api/v1/admin/analytics/assessments/lowest-performing
        [HttpGet("admin/analytics/assessments/lowest-performing")]
        public async Task<IActionResult> GetLowestPerformingAssessments()
        {
            var result = await assessmentService.GetLowestAssessments();
            return Ok(result);
        }
        [HttpGet("admin/assessments/metrics")]
        public async Task<IActionResult> GetAssessmentMetrics([FromQuery] Guid? instructorId = null, [FromQuery] Guid? batchId = null)
        {
            var result = await _dashBoardService.GetAssessmentMetrics(instructorId, batchId);
            return Ok(result);
        }
        [HttpGet("admin/analytics/score-by-question-type")]

        public async Task<IActionResult> GetScoreByQuestionType([FromQuery] Guid? batchId = null, [FromQuery] Guid? instructorId = null)
        {
            var result = await _dashBoardService.GetQuestionTypeMetrics(batchId, instructorId);
            return Ok(result);
        }
        [HttpGet("admin/analytics/assessments/score-trends")]
        public async Task<IActionResult> GetScoreTrends([FromQuery] Guid? instructorId, [FromQuery] Guid? batchId)
        {
            var result = await _dashBoardService.GetScoreTrendsAsync(instructorId, batchId);
            return Ok(result);
        }

        [HttpGet("admin/analytics/assessments/created-trend")]
        public async Task<IActionResult> GetAssessmentCreatedTrend([FromQuery] Guid? instructorId, [FromQuery] Guid? batchId)
        {
            var result = await _dashBoardService.GetAssessmentsCreatedTrendAsync(instructorId, batchId);
            return Ok(result);
        }
        [HttpGet("admin/batches/analytics")]
        public async Task<IActionResult> GetBatchAnalyticsMetrics([FromQuery] Guid? batchId)
        {
            var result = await _batchService.GetBatchAnalyticsMetrics(batchId);
            return Ok(result);
        }
        [HttpGet("admin/batches/performance-trend")]
        public async Task<IActionResult> GetBatchPerformanceTrend([FromQuery] Guid? batchId)
        {
            var result = await _batchService.GetBatchPerformanceTrend(batchId);
            return Ok(result);
        }


    }
}
