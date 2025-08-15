using Application.Dtos;
using Domain.Enum;

namespace Application.Interfaces.Services
{
    public interface IAssessmentService
    {
        Task<BaseResponse<AssessmentDto>> CreateAssessmentAsync(CreateAssessmentRequestModel model);
        Task<BaseResponse<AssessmentDto>> GetAssessmentAsync(Guid id);
        Task<BaseResponse<AssessmentMetrics>> GetAssessmentMetrics(Guid id);
        Task<BaseResponse<List<BatchPerformance>>> GetBatchPerformance(Guid id);

        Task<BaseResponse<List<AssessmentScoreDistribution>>> GetAssessmentScoreDistribution(Guid assessmentId);
        Task<BaseResponse<PaginationDto<StudentAssessmeentPerformance>>> GetStudentAssessmentPerformance(Guid assessmentId, PaginationRequest request);
        Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAllAssessmentsAsync(PaginationRequest request);
        Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAllAssessmentsByInstructorIdAsync( PaginationRequest request, Guid instructorId = default);
        Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAllStudentAssessments(PaginationRequest request, Guid studentId = default);
        Task<BaseResponse<PaginationDto<AssessmentDto>>> GetCurrentStudentAssessments(
     PaginationRequest request, AssessmentStatus? status);
        Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAssessmentsByStudentId(Guid studentId, PaginationRequest request);
        //Task<BaseResponse<AssessmentDto>> AssignStudents(Guid id, AssignStudentsModel model);
        Task<BaseResponse<AssessmentDto>> UpdateAssessmentAsync(Guid id, UpdateAssessmentRequestModel model);
        Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAssessmentsByBatchId(Guid id, PaginationRequest request);
        Task<BaseResponse<List<AssessmentPerformanceDto>>> GetTopAssessments();
        Task<BaseResponse<List<AssessmentPerformanceDto>>> GetLowestAssessments();
        Task<BaseResponse<List<AssessmentDto>>> GetRecentAssessment();
        Task<BaseResponse<List<AssessmentPerformanceDto>>> GetInstructorAssessmentScoresAsync(DateTime? fromDate, DateTime? toDate);
        Task<BaseResponse<PaginationDto<InstructorAssessmentDto>>> GetAssessmentsByInstructorAsync(Guid? batchId, AssessmentStatus? status, PaginationRequest request);
        Task<BaseResponse<PaginationDto<BatchAssessmentsOverview>>> GetAssessmentsByBatchIdDetails(Guid id, PaginationRequest request);
        Task<BaseResponse<PaginationDto<StudentAssessmentDetail>>> GetStudentAssessmentDetails(Guid id,PaginationRequest request);
        Task<BaseResponse<PaginationDto<InstructorAssessmentPerformanceDetailDto>>> GetInstructorAssessmentDetail(Guid instructorId, PaginationRequest request);
        Task<BaseResponse<List<AssessmentDto>>> GetInstructorRecentAssessment();
        Task UpdateAssessmentStatusAsync(Guid assessmentId, AssessmentStatus newStatus);
        Task<BaseResponse<PaginationDto<AdminAssessmentDto>>> GetAllAssessmentsAsync(
    Guid? batchId, DateTime? startDate, DateTime? endDate, string search, PaginationRequest request);
        Task<AssessmentOverviewDto> GetAssessmentOverviewAsync(Guid assessmentId);
        Task<BaseResponse<PaginationDto<GroupedStudentDto>>> GetGroupedStudentsAsync(Guid assessmentId, AssessmentStudentGroupType type, PaginationRequest request);
    }
}
