using Application.Dtos;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<BaseResponse<UserDto>> RegisterInstructor(RegisterIstructorRequestModel model);
        Task<BaseResponse<UserDto>> RegisterStudents(BulkRegisterUserRequestModel model);
        Task<BaseResponse<UserDto>> RegisterStudent(RegisterUserRequestModel model);
        Task<BaseResponse<UserDto>> GetAsync(Guid id);
        Task<BaseResponse<PaginationDto<UserDto>>> GetAllByBatchId(Guid id, PaginationRequest request);
        Task<BaseResponse<UserDto>> UpdateUser(UpdateUserRequsteModel model);
        Task<BaseResponse<UserDto>> DeleteAsync(Guid id);
        Task<BaseResponse<List<UserDto>>> SearchByNameOrEmailAsync(string query, string? status = null);
        Task<BaseResponse<PaginationDto<LeaderboardDto>>> GetLeaderboardAsync(Guid? batchId, PaginationRequest request);
        Task<BaseResponse<List<UserDto>>> SearchInstructorByNameOrEmailAsync(string query, string? status = null);
        Task<BaseResponse<StudentDetail>> GetStudentDetail(Guid id);
        Task<BaseResponse<StudentAnalytics>> GetStudentAnalytics(Guid id);
        Task<BaseResponse<UserDto>> UpdateStudentBatch(Guid studentId, Guid newBatchId);
        Task<BaseResponse<UserDto>> UpdateStudentStatusAsync(Guid studentId, string newStatus);
    }
}
