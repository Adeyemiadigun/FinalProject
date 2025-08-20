using Application.Dtos;

namespace Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<BaseResponse<InstructorDashboardOverview>> GetInstructorDashboardAsync();
        Task<BaseResponse<AdminDashBoardOverview>> AdminDashBoardOverview();
        Task<BaseResponse<AssessmentMetricsDto>> GetAssessmentMetrics(Guid? instructorId , Guid? batchId);
        Task<BaseResponse<StudentDashBoardDto>> GetStudentDashboardAsync();
        Task<BaseResponse<List<QuestionTypeMetrics>>> GetQuestionTypeMetrics(Guid? instructorId, Guid? batchId);
        Task<BaseResponse<List<BatchAnalytics>>> BatchAnalytics(Guid? instructorId, Guid? batchId);
        Task<BaseResponse<List<ScoreTrenddto>>> GetScoreTrendsAsync(Guid? instructorId, Guid? batchId, DateTime? month);
        Task<BaseResponse<List<AssessmentCreatedDto>>> GetAssessmentsCreatedTrendAsync(Guid? instructorId, Guid? batchId, DateTime? month);
        Task<BaseResponse<LeaderboardSummaryDto>> GetStudentDashboardLeaderboardSummaryAsync();
    }

}