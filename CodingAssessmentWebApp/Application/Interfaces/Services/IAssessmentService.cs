using Application.Dtos;

namespace Application.Interfaces.Services
{
    public interface IAssessmentService
    {
        Task<BaseResponse<AssessmentDto>> CreateAssessmentAsync(CreateAssessmentRequestModel model);
        Task<BaseResponse<AssessmentDto>> GetAssessmentAsync(Guid id);
        Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAllAssessmentsAsync(PaginationRequest request);
        Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAllAssessmentsByInstructorIdAsync( PaginationRequest request, Guid instructorId = default);
        Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAllStudentAssessments(PaginationRequest request, Guid studentId = default);
        Task<BaseResponse<PaginationDto<AssessmentDto>>> GetCurrentStudentAssessments(PaginationRequest request);
        Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAssessmentsByStudentId(Guid studentId, PaginationRequest request);
        Task<BaseResponse<AssessmentDto>> AssignStudents(Guid id, AssignStudentsModel model);
        Task<BaseResponse<AssessmentDto>> UpdateAssessmentAsync(Guid id, UpdateAssessmentRequestModel model);
        Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAssessmentsByBatchId(Guid id, PaginationRequest request);
        Task<BaseResponse<List<AssessmentPerformanceDto>>> GetTopAssessments();
        Task<BaseResponse<List<AssessmentPerformanceDto>>> GetLowestAssessments();
        Task<BaseResponse<List<AssessmentDto>>> GetRecentAssessment();
        Task<BaseResponse<List<AssessmentPerformanceDto>>> GetInstructorAssessmentScoresAsync();
        Task<PaginationDto<InstructorAssessmentDto>> GetAssessmentsByInstructorAsync(Guid? batchId, string? status, PaginationRequest request);
        Task<BaseResponse<PaginationDto<BatchAssessmentsOverview>>> GetAssessmentsByBatchIdDetails(Guid id, PaginationRequest request);
        Task<BaseResponse<PaginationDto<StudentAssessmentDetail>>> GetStudentAssessmentDetails(Guid id,PaginationRequest request);
    }
}
