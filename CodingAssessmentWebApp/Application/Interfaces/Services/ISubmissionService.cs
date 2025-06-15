using Application.Dtos;
using Domain.Entitties;

namespace Application.Interfaces.Services
{
    public interface ISubmissionService
    {
        Task<BaseResponse<SubmissionDto>> SubmitAssessment(Guid assessmentId, AnswerSubmissionDto submission);
        Task<Submission> GetAsync(Guid id);
        Task<Submission> GetWithAnswersAsync(Guid id);
        Task<PaginationDto<Submission>> GetAllAsync(Guid assessmentId, PaginationRequest request);
        Task<PaginationDto<Submission>> GetStudentSubmissionsAsync(Guid studentId, PaginationRequest request);
        Task<bool> CheckSubmissionExistsAsync(Guid id);
        Task<bool> CheckSubmissionExistsAsync(Guid studentId, Guid assessmentId);
    }
}
