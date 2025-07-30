using System.Threading.Tasks;
using Application.Dtos;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<BaseResponse<UserDto>> RegisterInstructor(RegisterIstructorRequestModel model);
        Task<BaseResponse<UserDto>> UploadStudentFileAsync(UploadFileDto studentFile);
        Task<BaseResponse<UserDto>> RegisterStudents(BulkRegisterUserRequestModel model);
        Task<BaseResponse<List<UserDto>>> GetInstructors();
        Task<BaseResponse<UserDto>> RegisterStudent(RegisterUserRequestModel model);
        Task<BaseResponse<UserDto>> GetAsync(Guid id);
        Task<BaseResponse<PaginationDto<UserDto>>> GetAllByBatchId(Guid id, PaginationRequest request);
        Task<BaseResponse<PaginationDto<UserDto>>> GetAllByBatchId(Guid? batchId, PaginationRequest request);
        
            Task<BaseResponse<UserDto>> UpdateUser(UpdateUserRequsteModel model);
        Task<BaseResponse<UserDto>> DeleteAsync(Guid id);
        Task<BaseResponse<PaginationDto<UserDto>>> SearchByNameOrEmailAsync(Guid? batchId,string? query, PaginationRequest request, string? status = null);
        Task<BaseResponse<PaginationDto<LeaderboardDto>>> GetLeaderboardAsync(Guid? batchId, PaginationRequest request);
        Task<BaseResponse<PaginationDto<LeaderboardDto>>> GetStudentBatchLeaderboardAsync(PaginationRequest request);
        Task<BaseResponse<PaginationDto<UserDto>>> SearchInstructorByNameOrEmailAsync(string? query, PaginationRequest request, string? status = null);
        Task<BaseResponse<StudentDetail>> GetStudentDetail(Guid id);
        Task<BaseResponse<StudentAnalytics>> GetStudentAnalytics(Guid id);
        Task<BaseResponse<UserDto>> UpdateStudentBatch(Guid studentId, Guid newBatchId);
        Task<BaseResponse<UserDto>> UpdateStudentStatusAsync(Guid studentId, string newStatus);
        Task<BaseResponse<InstructorDetailsDto>> GetInstructorDetails(Guid instructorId);
        Task<BaseResponse<StudentDashboardSummaryDto>> GetSummaryAsync( );
        Task<BaseResponse<List<StudentAssessmentDto>>> GetOngoingAssessmentsAsync( );
        Task<BaseResponse<List<StudentAssessmentDto>>> GetUpcomingAssessmentsAsync();
        Task<BaseResponse<List<StudentScoreTrendDto>>> GetStudentScoreTrendsAsync();
        Task<BaseResponse<List<SubmissionDto>>> GetSubmittedAssessmentsAsync();
        Task<BaseResponse<StudentDetail>> GetStudentDetail();
        Task<BaseResponse<StudentProfileMetrics>> GetStudentMetrics();
        Task<BaseResponse<PaginationDto<SubmissionsDto>>> GetStudentSubmissionsAsync(Guid studentId, PaginationRequest request);
    }
}
