using Application.Dtos;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<BaseResponse<UserDto>> LoginAsync(LoginRequestModel user);
        Task<BaseResponse<UserDto>> RegisterInstructor(RegisterUserRequestModel model);
        Task<BaseResponse<UserDto>> RegisterStudents(BulkRegisterUserRequestModel model);
        Task<BaseResponse<UserDto>> GetAsync(Guid id);
        Task<BaseResponse<UserDto>> UpdateUser(UpdateUserRequsteModel model);
        Task<BaseResponse<UserDto>> DeleteAsync(Guid id);
    }
}
