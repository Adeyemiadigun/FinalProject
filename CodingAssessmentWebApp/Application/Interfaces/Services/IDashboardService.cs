using Application.Dtos;

namespace Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<BaseResponse<StudentDashBoardDto>> GetInstructorDashboardAsync();
        Task<BaseResponse<AdminDashBoardOverview>> AdminDashBoardOverview();
        Task<BaseResponse<AssessmentMetricsDto>> GetAssessmentMetrics(Guid? instructorId , Guid? batchId);
        Task<BaseResponse<StudentDashBoardDto>> GetStudentDashboardAsync();
        Task<BaseResponse<List<QuestionTypeMetrics>>> GetQuestionTypeMetrics(Guid? instructorId, Guid? batchId);
        Task<BaseResponse<List<BatchAnalytics>>> BatchAnalytics(Guid? instructorId, Guid? batchId);
        Task<BaseResponse<List<ScoreTrenddto>>> GetScoreTrendsAsync(Guid? instructorId, Guid? batchId);
        Task<BaseResponse<List<AssessmentCreatedDto>>> GetAssessmentsCreatedTrendAsync(Guid? instructorId, Guid? batchId);
    }

}
//🧠 1.GET / api / v1 / admin / assessments / metrics ? batchId = &instructorId =
//🎯 Fills the metric cards at the top of the page.

//✅ Return:
//json
//Copy
//Edit
//{
//  "totalAssessments": 32,
//  "activeAssessments": 14,
//  "averageScore": 75.6,
//  "passRate": 82.3,
//  "completionRate": 91.5
//}
//📌 Used For:
//🟦 Total Assessments

//🟨 Active Assessments

//🟩 Avg Score

//🟧 Pass Rate

//🟪 Completion Rate