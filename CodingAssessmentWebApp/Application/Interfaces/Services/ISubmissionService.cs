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
       Task<PaginationDto<Submission>> GetStudentSubmissionsAsync(Guid studentId, PaginationRequest request);
    }
}
