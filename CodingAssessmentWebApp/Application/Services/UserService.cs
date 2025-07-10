using System.Net.WebSockets;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Entitties;
using Domain.Enum;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBatchRepository _batchRepository;
        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, IBatchRepository batchRepository)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _batchRepository = batchRepository;
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

        public async Task<BaseResponse<UserDto>> RegisterInstructor(RegisterIstructorRequestModel model)
        {
            var user = await _userRepository.CheckAsync(x => x.Email == model.Email);
            if (user)
                throw new ApiException("User with the provided email already exists", 400, "DuplicateEmail", null); // Fix for CS7036 and CS1002

            var newUser = new User()
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = Role.Instructor,
                IsActive = true,
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

        public async Task<BaseResponse<UserDto>> RegisterStudents(BulkRegisterUserRequestModel model)
        {
            var emails = model.Users.Select(x => x.Email).ToList();
            var existingUsers = await _userRepository.GetAllAsync(user => emails.Contains(user.Email));
            if (existingUsers.Any())
                throw new ApiException("One or more users already exist with the provided emails", 400, "DuplicateEmails", null); // Fix for CS7036 and CS1002

            if (model.Users == null || !model.Users.Any())
                throw new ApiException("User list cannot be empty", 400, "EmptyUserList", null); // Fix for CS7036 and CS1002
      
            var users = new List<User>();
            foreach (var user in model.Users)
            {
                var newUser = new User()
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password),
                    Role = Role.Student,
                    IsActive = true,
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
        public async Task<BaseResponse<UserDto>> RegisterStudent(RegisterUserRequestModel model)
        {
            var user = await _userRepository.CheckAsync(x => x.Email == model.Email);
            if (user)
                throw new ApiException("User with the provided email already exists", 400, "DuplicateEmail", null); // Fix for CS7036 and CS1002
            var batch = await _batchRepository.GetBatchByIdAsync(model.BatchId);
            var newUser = new User()
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = Domain.Enum.Role.Student,
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

        public async Task<BaseResponse<UserDto>> UpdateUser(UpdateUserRequsteModel model)
        {
            var user = await _userRepository.GetAsync(model.Id);
            if (user == null)
                throw new ApiException("User not found", 404, "UserNotFound", null);

            var check = await _userRepository.CheckAsync(x => x.Email == model.Email && x.Id != user.Id);
            if (check)
                throw new ApiException("Email already exists for another user", 400, "DuplicateEmail", null); 

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

        public async Task<BaseResponse<PaginationDto<UserDto>>> GetAllByBatchId(Guid id, PaginationRequest request)
        {
            var users = await _userRepository.GetAllAsync(x => x.BatchId == id,request);
            if (users == null)
                throw new ApiException("User not found", 404, "UserNotFound", null);

            var userDto = users.Items.Select(x => new  UserDto()
            {
                Id = x.Id,
                Email = x.Email,
                Role = x.Role,
                FullName = x.FullName,
            });
            var paginationDto = new PaginationDto<UserDto>
            {
                Items = userDto,
                TotalItems = users.TotalItems,
                TotalPages = users.TotalPages,
                CurrentPage = users.CurrentPage,
                PageSize = users.PageSize,
                HasNextPage = users.HasNextPage,
                HasPreviousPage = users.HasPreviousPage
            };
            return new BaseResponse<PaginationDto<UserDto>>()
            {
                Message = "Users retrieved successfully",
                Status = true,
                Data = paginationDto
            };
        }

        public async Task<BaseResponse<List<UserDto>>> SearchByNameOrEmailAsync(string query, string? status = null)
        {
            var normalizedQuery = query.Trim().ToLower();

            var users = await _userRepository.GetAllAsync(s =>
                (s.Role == Role.Student) &&
                (string.IsNullOrEmpty(normalizedQuery) ||
                 s.FullName.ToLower().Contains(normalizedQuery) ||
                 s.Email.ToLower().Contains(normalizedQuery)) &&
                (status == null || (status == "active" && s.IsActive) || (status == "inactive" && !s.IsActive))
            );

            var userDtos = users.Select(s => new UserDto
            {
                Id = s.Id,
                FullName = s.FullName,
                Email = s.Email,
            }).ToList();

            return new BaseResponse<List<UserDto>>()
            {
                Message = "Users retrieved successfully",
                Status = true,
                Data = userDtos
            };
        }
        public async Task<BaseResponse<List<UserDto>>> SearchInstructorByNameOrEmailAsync(string query, string? status = null)
        {
            var normalizedQuery = query?.Trim().ToLower() ?? string.Empty;

            var users = await _userRepository.GetAllAsync(s =>
                (s.Role == Role.Instructor) &&
                (string.IsNullOrEmpty(normalizedQuery) ||
                 s.FullName.ToLower().Contains(normalizedQuery) ||
                 s.Email.ToLower().Contains(normalizedQuery)) &&
                (status == null || (status == "active" && s.IsActive) || (status == "inactive" && !s.IsActive))
            );

            var userDtos = users.Select(s => new UserDto
            {
                Id = s.Id,
                FullName = s.FullName,
                Email = s.Email,
            }).ToList();

            return new BaseResponse<List<UserDto>>()
            {
                Message = "Users retrieved successfully",
                Status = true,
                Data = userDtos
            };
        }

        public async Task<BaseResponse<PaginationDto<LeaderboardDto>>> GetLeaderboardAsync(Guid? batchId, PaginationRequest request)
        {
            var students = await _userRepository.GetAllWithRelationshipAsync(
                s => !batchId.HasValue || s.BatchId == batchId, request
            );

            if (students.Items == null || !students.Items.Any())
            {
                var emptyPagination = new PaginationDto<LeaderboardDto>
                {
                    Items = new List<LeaderboardDto>(),
                    TotalItems = 0,
                    TotalPages = 0,
                    CurrentPage = 0,
                    PageSize = request.PageSize,
                    HasNextPage = false,
                    HasPreviousPage = false
                };

                return new BaseResponse<PaginationDto<LeaderboardDto>>
                {
                    Message = "No students found",
                    Status = true,
                    Data = emptyPagination
                };
            }

            var leaderboard = students.Items
                .Where(s => s.Submissions.Any())
                .Select(s => new LeaderboardDto
                {
                    Id = s.Id,
                    Name = s.FullName,
                    Batch = s.Batch.Name,
                    AvgScore = Math.Round(s.Submissions.Average(sub => sub.TotalScore), 2),
                    HighestScore = s.Submissions.Max(sub => sub.TotalScore),
                    CompletedAssessments = s.Submissions.Count
                })
                .OrderByDescending(l => l.AvgScore)
                .ToList();

            var paginatedResponse = new PaginationDto<LeaderboardDto>
            {
                TotalItems = students.TotalItems,
                TotalPages = students.TotalPages,
                CurrentPage = students.CurrentPage,
                HasNextPage = students.HasNextPage,
                HasPreviousPage = students.HasPreviousPage,
                PageSize = students.PageSize,
                Items = leaderboard
            };

            return new BaseResponse<PaginationDto<LeaderboardDto>>
            {
                Message = "Leaderboard retrieved successfully",
                Status = true,
                Data = paginatedResponse
            };
        }
        public async Task<BaseResponse<StudentDetail>> GetStudentDetail(Guid id)
        {
            if (Guid.Empty == id)
                throw new ApiException("Invalid student ID provided", 400, "InvalidId", null);


            var student = await _userRepository.GetAsync(id)
                          ?? throw new ApiException("Student not found", 404, "StudentNotFound", null);

            return new BaseResponse<StudentDetail>
            {
                Message = "Student details retrieved successfully",
                Status = true,
                Data = new StudentDetail
                {
                    Id = student.Id,
                    FullName = student.FullName,
                    Email = student.Email,
                    BatchName = student.Batch?.Name,
                    Status = student.IsActive
                }
            };
        }
        public async Task<BaseResponse<StudentAnalytics>> GetStudentAnalytics(Guid id)
        {
            if (Guid.Empty == id)
                throw new ApiException("Invalid student ID provided", 400, "InvalidId", null);

            var student = await _userRepository.GetAsync(x => x.Id == id)
                                  ?? throw new ApiException("Student analytics not found", 404, "AnalyticsNotFound", null);
            var studentAnalytics = new StudentAnalytics()
            {
                TotalAssessments = student.Assessments.Count,
                Attempted = student.Submissions.Count(),
                AvgScore = student.Submissions.Any() ? Math.Round(student.Submissions.Average(s => s.TotalScore), 2) : 0,
                PassRate = student.Submissions.Count(s => s.Assessment.PassingScore <= s.TotalScore) / (double)student.Assessments.Count * 100
            };
            return new BaseResponse<StudentAnalytics>
            {
                Message = "Student analytics retrieved successfully",
                Status = true,
                Data =studentAnalytics
            };
        }
        public async Task<BaseResponse<UserDto>> UpdateStudentBatch(Guid studentId, Guid newBatchId)
        {
            if (Guid.Empty == studentId || Guid.Empty == newBatchId)
                throw new ApiException("Invalid student or batch ID provided", 400, "InvalidId", null);
            var student = await _userRepository.GetAsync(studentId)
                          ?? throw new ApiException("Student not found", 404, "StudentNotFound", null);
            var newBatch = await _batchRepository.GetBatchByIdAsync(newBatchId)
                          ?? throw new ApiException("New batch not found", 404, "BatchNotFound", null);
            student.BatchId = newBatch.Id;
            _userRepository.Update(student);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse<UserDto>
            {
                Message = "Student batch reassigned successfully",
                Status = true,
                Data = new UserDto
                {
                    Id = student.Id,
                    Email = student.Email,
                    Role = student.Role,
                    FullName = student.FullName,
                }
            };
        }
        public async Task<BaseResponse<UserDto>> UpdateStudentStatusAsync(Guid studentId, string newStatus)
        {
            if (Guid.Empty == studentId)
                throw new ApiException("Invalid student ID provided", 400, "InvalidId", null);

            var student = await _userRepository.GetAsync(studentId)
                          ?? throw new ApiException("Student not found", 404, "StudentNotFound", null);

            if (newStatus.ToLower() != "ictive" && newStatus.ToLower() != "inactive")
                throw new ApiException("Invalid status value", 400, "InvalidStatus", null);

            student.IsActive = newStatus.ToLower() == "active"; 
            _userRepository.Update(student);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse<UserDto>
            {
                Message = "Student status updated successfully",
                Status = true,
                Data = new UserDto
                {
                    Id = student.Id,
                    Email = student.Email,
                    Role = student.Role,
                    FullName = student.FullName,
                }
            };
        }

    }
}
