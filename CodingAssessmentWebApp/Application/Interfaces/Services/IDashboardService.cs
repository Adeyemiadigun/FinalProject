using Application.Dtos;

namespace Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<BaseResponse<StudentDashBoardDto>> GetInstructorDashboardAsync();
        Task<BaseResponse<StudentDashBoardDto>> GetStudentDashboardAsync();
    }

}
