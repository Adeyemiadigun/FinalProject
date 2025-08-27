using Application.Dtos;

namespace Application.Interfaces.Services
{
    public interface IStudentProgressService
    {
        Task SaveProgressAsync(SaveProgressDto saveProgressDto);
        Task<BaseResponse<LoadProgressDto>> GetProgressAsync(Guid assessmentId);
        Task DeleteProgressAsync(Guid progressId, Guid studentId);
    }
}
