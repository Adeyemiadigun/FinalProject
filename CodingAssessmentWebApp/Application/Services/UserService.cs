using System.Net.WebSockets;
using Application.Dtos;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entitties;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<UserDto>> DeleteAsync(Guid id)
        {
            var user = await _userRepository.GetAsync(id);
            if (user == null)
            {
                return new BaseResponse<UserDto>()
                {
                    Message = "User not found",
                    Status = false
                };
            }
            user.IsActive = false;
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse<UserDto>()
            {
                Message = "User deleted successfully",
                Status = true,
                Data = new UserDto()
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = user.Role,
                    FullName = user.FullName,
                }
            };
        }

        public async Task<BaseResponse<UserDto>> GetAsync(Guid id)
        {
            var user = await _userRepository.GetAsync(id);
            if (user == null)
            {
                return new BaseResponse<UserDto>()
                {
                    Message = "User not found",
                    Status = false
                };
            }
            return new BaseResponse<UserDto>()
            {
                Message = "User found",
                Status = true,
                Data = new UserDto()
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = user.Role,
                    FullName = user.FullName,
                }
            };
        }

        public async Task<BaseResponse<UserDto>> RegisterInstructor(RegisterUserRequestModel model)
        {
            var user = await _userRepository.CheckAsync(x => x.Email == model.Email);
            if (user)
            {
                return new BaseResponse<UserDto>()
                {
                    Message = "Email already exists",
                    Status = false
                };
            }
            var newUser = new User()
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = Domain.Enum.Role.Instructor,
                IsActive = false,
            };
            await _userRepository.CreateAsync(newUser);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse<UserDto>()
            {
                Message = "User created successfully",
                Status = true,
                Data = new UserDto()
                {
                   Id = newUser.Id,
                   Email = model.Email,
                   Role = newUser.Role,
                },
            };
            
        }

        public async  Task<BaseResponse<UserDto>> RegisterStudents(BulkRegisterUserRequestModel model)
        {
            var emails = model.Users.Select(x => x.Email).ToList();
            var existingUsers = await _userRepository.GetAllAsync(user => emails.Contains(user.Email));
            if (existingUsers.Any())
            {
                return new BaseResponse<UserDto>()
                {
                    Message = "Some emails already exist",
                    Status = false
                };
            }

            var users = new List<User>();
            foreach (var user in model.Users)
            {
                var newUser = new User()
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password),
                    Role = Domain.Enum.Role.Student,
                    IsActive = false,
                };
                users.Add(newUser);
            }
            await _userRepository.CreateAsync(users);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse<UserDto>()
            {
                Message = "Users created successfully",
                Status = true,
            };
        }

        public async Task<BaseResponse<UserDto>> UpdateUser(UpdateUserRequsteModel model)
        {
            var user = await _userRepository.GetAsync(model.Id);
            if (user == null)
            {
                return new BaseResponse<UserDto>()
                {
                    Message = "User not found",
                    Status = false
                };
            }
            var check = await _userRepository.CheckAsync(x => x.Email == model.Email && x.Id != user.Id);
            if (check)
            {
                return new BaseResponse<UserDto>()
                {
                    Message = "Email already exists",
                    Status = false
                };
            }
            user.FullName = model.FullName;
            user.Email = model.Email;
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse<UserDto>()
            {
                Message = "User updated successfully",
                Status = true,
                Data = new UserDto()
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = user.Role,
                    FullName = user.FullName,
                }
            };
        }
    }
}
