using System.Threading.Tasks;
using Application.Dtos;
using Domain.Entitties;

namespace Application.Interfaces.Services
{
    public interface ISubmissionService
    {
        Task<BaseResponse<SubmissionDto>> SubmitAssessment(Guid assessmentId, AnswerSubmissionDto submission);
        Task<BaseResponse<SubmissionDto>> GetStudentSubmissionAsync(Guid assessmentId, Guid studentId);
        Task<Submission> GetAsync(Guid id);
        Task<Submission> GetWithAnswersAsync(Guid id);
        Task<PaginationDto<Submission>> GetAllAsync(Guid assessmentId, PaginationRequest request);
        Task<BaseResponse<SubmissionDto>> GetCurrentStudentSubmission(Guid assessmentId);
        Task<BaseResponse<PaginationDto<SubmissionDto>>> GetCurrentStudentSubmission(PaginationRequest request);
        Task<BaseResponse<PaginationDto<SubmissionDto>>> GetAssessmentSubmissions(Guid assessmentId, PaginationRequest request);
        Task<BaseResponse<SubmissionDto>> GetSubmissionByIdAsync(Guid submissionId);
        Task<BaseResponse<List<StudentScoreTrendDto>>> GetStudentScoreTrendsAsync(Guid studentId, DateTime? date);
        Task<BaseResponse<PaginationDto<AssessmentHistoryItemDto>>> GetStudentAssessmentHistoryAsync(Guid studentId, string? TitleSearch, PaginationRequest request);
    }
}
