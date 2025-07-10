using Application.Dtos;

namespace Application.Interfaces.Services
{
    public interface IBatchService
    {
        Task<BaseResponse<BatchDto>> CreateBatchAsync(CreateBatchRequestModel model);
        Task<BaseResponse<BatchDto>> GetBatchByIdAsync(Guid id);
        Task<BaseResponse<PaginationDto<BatchDto>>> GetAllBatchesAsync(PaginationRequest request);
        Task<BaseResponse<List<BatchDto>>> GetAllBatchesAsync();
        Task<BaseResponse<BatchDto>> UpdateBatchAsync(Guid id, UpdateBatchRequestModel model);
        Task<BaseResponse<BatchDto>> DeleteBatchAsync(Guid id);
        Task<BaseResponse<List<BatchAnalytics>>> BatchAnalytics();
        Task<BaseResponse<string>> AssignAssessmentToBatchAsync(Guid batchId, Guid assessmentId);
        Task<BaseResponse<BatchAnalyticsMetricsDto>> GetBatchAnalyticsMetrics(Guid? batchId);
        Task<BaseResponse<BatchPerformanceTrendDto>> GetBatchPerformanceTrend(Guid? batchId);
        Task<BaseResponse<List<BatchSummaryDto>>> GetBatchSummariesAsync();
        Task<BaseResponse<List<BatchStudentCountDto>>> GetBatchStudentCountsAsync();
        Task<BaseResponse<BatchDetails>> GetBatchDetails(Guid batchId);
    }
}
