using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.WebSockets;
using System.Reflection;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Entitties;
using Domain.Enum;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBatchRepository _batchRepository;
        private readonly ICurrentUser _currentUser;
        private readonly ISubmissionRepository _submissionRepo;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly ILeaderboardStore _leaderboardStore;
        private readonly IConfiguration _config;
        private readonly string Password;

        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, IBatchRepository batchRepository, ICurrentUser currentUser, ISubmissionRepository submissionRepo, IAssessmentRepository assessmentRepository, ILeaderboardStore leaderboardStore, IConfiguration config)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _batchRepository = batchRepository;
            _currentUser = currentUser;
            _submissionRepo = submissionRepo;
            _assessmentRepository = assessmentRepository;
            _leaderboardStore = leaderboardStore;
            _config = config;
            Password = _config.GetSection("UserPassword").Value!;
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
                    FullName = newUser.FullName,
                    Email = model.Email,
                    Role = newUser.Role,
                    DateCreated = newUser.CreatedAt
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
                var batch = await _batchRepository.GetBatchByIdAsync(user.BatchId);
                if (batch is null)
                    throw new ApiException("Batch not found", 404, "BatchNotFound", null);
                var newUser = new User()
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password),
                    Role = Role.Student,
                    BatchId = batch.Id,
                    Batch = batch,
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
            if(batch is null)
                throw new ApiException("Batch not found", 404, "BatchNotFound", null); // Fix for CS7036 and CS1002
            var newUser = new User()
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = Domain.Enum.Role.Student,
                BatchId = batch.Id,
                Batch = batch,
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

        public async Task<BaseResponse<UserDto>> UpdateUser(UpdateUserRequestModel model)
        {
            var userId = _currentUser.GetCurrentUserId();
            var user = await _userRepository.GetAsync(userId);
            if (user == null)
                throw new ApiException("User not found", 404, "UserNotFound", null);

            var emailTaken = await _userRepository.CheckAsync(x => x.Email == model.Email && x.Id != user.Id);
            if (emailTaken)
                throw new ApiException("Email already exists for another user", 400, "DuplicateEmail", null);

            user.FullName = model.FullName;
            user.Email = model.Email;

            // Handle password change if requested
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                    throw new ApiException("Current password is required to change your password", 400, "CurrentPasswordRequired", null);

                var passwordValid = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash);
                if (!passwordValid)
                    throw new ApiException("Current password is incorrect", 400, "IncorrectPassword", null);

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse<UserDto>()
            {
                Status = true,
                Message = "User updated successfully",
                Data = new UserDto()
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role
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
                Status = x.IsActive
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
        public async Task<BaseResponse<PaginationDto<UserDto>>> GetAllByBatchId(Guid? batchId, PaginationRequest request)
        {
            Expression<Func<User, bool>> predicate = batchId.HasValue
                ? (x => x.BatchId == batchId.Value)
                : (_ => true); 

            var users = await _userRepository.GetAllAsync(predicate, request);

            if (users == null || !users.Items.Any())
                throw new ApiException("No users found", 404, "UserNotFound", null);

            var userDto = users.Items.Select(x => new UserDto
            {
                Id = x.Id,
                Email = x.Email,
                Role = x.Role,
                FullName = x.FullName,
                Status = x.IsActive
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

            return new BaseResponse<PaginationDto<UserDto>>
            {
                Message = "Users retrieved successfully",
                Status = true,
                Data = paginationDto
            };
        }


        public async Task<BaseResponse<PaginationDto<UserDto>>> SearchByNameOrEmailAsync(
      Guid? batchId, string? query, PaginationRequest request, string? status = null)
        {
            var normalizedQuery = query?.Trim().ToLower() ?? "";

            var users = await _userRepository.GetAllAsync(s =>
                s.Role == Role.Student &&
                (string.IsNullOrEmpty(normalizedQuery) ||
                 s.FullName.ToLower().Contains(normalizedQuery) ||
                 s.Email.ToLower().Contains(normalizedQuery)) &&
                (batchId == null || s.BatchId == batchId) &&
                (string.IsNullOrEmpty(status) ||
                 (status == "active" && s.IsActive) ||
                 (status == "inactive" && !s.IsActive)),
                request
            );

            if (users == null || !users.Items.Any())
            {
                throw new ApiException("No users found", 404, "UserNotFound", null);
            }

            var userDtos = users.Items.Select(s => new UserDto
            {
                Id = s.Id,
                FullName = s.FullName,
                Email = s.Email,
                Status = s.IsActive
            }).ToList();

            var paginationDto = new PaginationDto<UserDto>
            {
                Items = userDtos,
                TotalItems = users.TotalItems,
                TotalPages = users.TotalPages,
                CurrentPage = users.CurrentPage,
                PageSize = users.PageSize,
                HasNextPage = users.HasNextPage,
                HasPreviousPage = users.HasPreviousPage
            };

            return new BaseResponse<PaginationDto<UserDto>>
            {
                Message = "Users retrieved successfully",
                Status = true,
                Data = paginationDto
            };
        }

        public async Task<BaseResponse<PaginationDto<UserDto>>> SearchInstructorByNameOrEmailAsync(string? query,PaginationRequest request, string? status = null)
        {
            var normalizedQuery = query?.Trim().ToLower() ?? string.Empty;

            var users = await _userRepository.GetAllAsync(s =>
                (s.Role == Role.Instructor) &&
                (string.IsNullOrEmpty(normalizedQuery) ||
                 s.FullName.ToLower().Contains(normalizedQuery) ||
                 s.Email.ToLower().Contains(normalizedQuery)) &&
                (status == null || (status == "active" && s.IsActive) || (status == "inactive" && !s.IsActive)), request
            );

            var userDtos = users.Items.Select(s => new UserDto
            {
                Id = s.Id,
                FullName = s.FullName,
                Email = s.Email,
                Status = s.IsActive
            }).ToList();
            var paginated = new PaginationDto<UserDto>()
            {
                Items = userDtos,
                TotalPages = users.TotalPages,
                TotalItems = users.TotalItems,
                HasNextPage = users.HasNextPage,
                HasPreviousPage = users.HasNextPage,
                CurrentPage = users.CurrentPage,
                PageSize = users.PageSize,

            };

            return new BaseResponse<PaginationDto<UserDto>>()
            {
                Message = "Users retrieved successfully",
                Status = true,
                Data = paginated
            };
        }
        public async Task<BaseResponse<PaginationDto<LeaderboardDto>>> GetLeaderboardAsync(Guid? batchId, PaginationRequest request)
        {
            if (!batchId.HasValue)
            {
                return new BaseResponse<PaginationDto<LeaderboardDto>>
                {
                    Status = false,
                    Message = "BatchId is required in this setup.",
                };
            }

            var batchIdValue = batchId.Value;

            var leaderboard = await _leaderboardStore.GetLeaderBoardByBatchId(batchIdValue);

            var skip = (request.CurrentPage - 1) * request.PageSize;
            var paginatedItems = leaderboard.Skip(skip).Take(request.PageSize).ToList();

            var paginatedResponse = new PaginationDto<LeaderboardDto>
            {
                Items = paginatedItems,
                TotalItems = leaderboard.Count,
                TotalPages = (int)Math.Ceiling((double)leaderboard.Count / request.PageSize),
                CurrentPage = request.CurrentPage,
                PageSize = request.PageSize,
                HasNextPage = skip + request.PageSize < leaderboard.Count,
                HasPreviousPage = request.CurrentPage > 1
            };

            return new BaseResponse<PaginationDto<LeaderboardDto>>
            {
                Status = true,
                Message = "Leaderboard retrieved successfully",
                Data = paginatedResponse
            };
        }
        public async Task<BaseResponse<PaginationDto<LeaderboardDto>>> GetStudentBatchLeaderboardAsync(PaginationRequest request)
        {
            var userId = _currentUser.GetCurrentUserId();

            var student = await _userRepository.GetAsync(userId);
            if (student == null || student.BatchId == null)
            {
                return new BaseResponse<PaginationDto<LeaderboardDto>>
                {
                    Status = false,
                    Message = "Student not found or not assigned to a batch"
                };
            }

            var batchId = student.BatchId.Value;

            var leaderboard = await _leaderboardStore.GetLeaderBoardByBatchId(batchId);

            var skip = (request.CurrentPage - 1) * request.PageSize;
            var paginatedItems = leaderboard.Skip(skip).Take(request.PageSize).ToList();

            var paginatedResponse = new PaginationDto<LeaderboardDto>
            {
                Items = paginatedItems,
                TotalItems = leaderboard.Count,
                TotalPages = (int)Math.Ceiling((double)leaderboard.Count / request.PageSize),
                CurrentPage = request.CurrentPage,
                PageSize = request.PageSize,
                HasNextPage = skip + request.PageSize < leaderboard.Count,
                HasPreviousPage = request.CurrentPage > 1
            };

            return new BaseResponse<PaginationDto<LeaderboardDto>>
            {
                Status = true,
                Message = "Batch leaderboard retrieved successfully",
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
        public async Task<BaseResponse<StudentDetail>> GetStudentDetail()
        {
            var id = _currentUser.GetCurrentUserId();
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
                    Status = student.IsActive,
                    DateCreated = student.CreatedAt
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
                TotalAssessments = student.AssessmentAssignments.Count,
                Attempted = student.Submissions.Count(),
                AvgScore = student.Submissions.Any() ? Math.Round(student.Submissions.Average(s => s.TotalScore), 2) : 0,
                PassRate = student.Submissions.Count(s => s.Assessment.RequiredPassingScore <= s.TotalScore) / (double)student.AssessmentAssignments.Count * 100
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

            if (newStatus.ToLower() != "active" && newStatus.ToLower() != "inactive")
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

        public async Task<BaseResponse<InstructorDetailsDto>> GetInstructorDetails(Guid instructorId)
        {
            if (instructorId == Guid.Empty)
                throw new ApiException("Invalid instructor ID provided", 400, "InvalidId", null);

            var instructor = await _userRepository.GetForInstructorAsync(instructorId)
                              ?? throw new ApiException("Instructor not found", 404, "InstructorNotFound", null);

            var instructorDetails = new InstructorDetailsDto
            {
                Id = instructor.Id,
                FullName = instructor.FullName,
                Email = instructor.Email,
                Status = instructor.IsActive,
                JoinedDate = instructor.CreatedAt,
                TotalAssessment = instructor.Assessments.Count,
                AverageScore = instructor.Assessments.Any()? instructor.Assessments
                    .Where(x => x.Submissions.Any())
                    .SelectMany(x => x.Submissions)
                    .Average(s => s.TotalScore) : 0
            };

            return new BaseResponse<InstructorDetailsDto>
            {
                Message = "Instructor details retrieved successfully",
                Status = true,
                Data = instructorDetails
            };
        }

        public async Task<BaseResponse<StudentDashboardSummaryDto>> GetSummaryAsync( )
        {
            var studentId = _currentUser.GetCurrentUserId();
            if (studentId == Guid.Empty)
                throw new ApiException("Invalid student ID provided", 400, "InvalidId", null);
            var submissions = await _submissionRepo.GetAllAsync(x => x.StudentId == studentId);
            if (!submissions.Any())
            throw new ApiException("NoSubmissionForStudent", 500, "UnknownError", null);
            var submissionsNotAutoSubmitted = submissions.Count(s => s.SubmittedAt != null && s.IsAutoSubmitted == false);
            var submissionCount = submissions.Count(); 
            return new BaseResponse<StudentDashboardSummaryDto>
            {
                Message = "Student dashboard summary retrieved successfully",
                Status = true,
                Data = new StudentDashboardSummaryDto
                {
                    TotalAssessments = submissions.Count,
                    AverageScore = submissions.Average(s => s.TotalScore),
                    HighestScore = submissions.Max(s => s.TotalScore),
                    CompletionRate = 100 * submissionsNotAutoSubmitted / submissionCount,
                    Completed = submissionsNotAutoSubmitted
                }
            };
        }

        public async Task<BaseResponse<List<StudentAssessmentDto>>> GetOngoingAssessmentsAsync()
        {
            var studentId = _currentUser.GetCurrentUserId();
            var check = await _userRepository.CheckAsync(x => x.Id == studentId);
            if (!check)
                throw new ApiException("Student not found", 404, "StudentNotFound", null);
            if (studentId == Guid.Empty)
                throw new ApiException("Invalid student ID provided", 400, "InvalidId", null);
            var assessments = await _assessmentRepository.GetAllAsync(x => x.AssessmentAssignments.Any(x => x.StudentId == studentId)&& x != null && x.Status == AssessmentStatus.InProgress, new PaginationRequest { CurrentPage = 1, PageSize = 4});

            if (!assessments.Items.Any())
                throw new ApiException("No ongoing assessments found for the student", 404, "NoOngoingAssessments", null);
            var ongoingAssessments = assessments.Items
                .Select(x => new StudentAssessmentDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    StartedDate = x.StartDate,
                    EndDate = x.EndDate,
                    Status = x.Status.ToString(),
                    Type = x.TechnologyStack.ToString(),
                    Duration = x.DurationInMinutes
                }).ToList();
            return new BaseResponse<List<StudentAssessmentDto>>
            {
                Message = "Ongoing assessments retrieved successfully",
                Status = true,
                Data = ongoingAssessments
            };
        }

        public async Task<BaseResponse<List<StudentAssessmentDto>>> GetUpcomingAssessmentsAsync()
        {
            var studentId = _currentUser.GetCurrentUserId();
            var student = await _userRepository.CheckAsync(x => x.Id == studentId);
            if (!student)
                throw new ApiException("Invalid student ID provided", 400, "InvalidId", null);
            var assessments = await _assessmentRepository.GetAllAsync(x => x.AssessmentAssignments.Any(x => x.StudentId == studentId)&& x.StartDate > DateTime.UtcNow, new PaginationRequest { CurrentPage = 1, PageSize = 4 });
            if (!assessments.Items.Any())
                throw new ApiException("No upcoming assessments found for the student", 404, "NoUpcomingAssessments", null);

            var upcomingAssessment = assessments.Items
               .Select(x => new StudentAssessmentDto
               {
                   Id = x.Id,
                   Title = x.Title,
                   StartedDate = x.StartDate,
                   EndDate = x.EndDate,
                   Status = x.Status.ToString(),
                   Type = x.TechnologyStack.ToString(),
                   Duration = x.DurationInMinutes
               }).ToList();
            return new BaseResponse<List<StudentAssessmentDto>>
            {
                Message = "Upcoming assessments retrieved successfully",
                Status = true,
                Data = upcomingAssessment
            };
        }

        public async Task<BaseResponse<List<StudentScoreTrendDto>>> GetStudentScoreTrendsAsync(DateTime? startDate, DateTime? endDate)
        {
            var studentId = _currentUser.GetCurrentUserId();

            if (studentId == Guid.Empty)
                throw new ApiException("Invalid student ID.", 403, "InvalidStudent", null);

            var submissions = await _submissionRepo.GetAllAsync(s =>
                s.StudentId == studentId &&
                s.SubmittedAt != default &&
                (!startDate.HasValue || s.SubmittedAt.Date >= startDate.Value.Date) &&
                (!endDate.HasValue || s.SubmittedAt.Date <= endDate.Value.Date)
            );

            if (!submissions.Any())
            {
                return new BaseResponse<List<StudentScoreTrendDto>>
                {
                    Message = "No submissions found to generate score trend.",
                    Status = true,
                    Data = new List<StudentScoreTrendDto>()
                };
            }

            var trends = submissions
                .GroupBy(s =>
                    CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                        s.SubmittedAt,
                        CalendarWeekRule.FirstFourDayWeek,
                        DayOfWeek.Monday))
                .OrderBy(g => g.Key)
                .Select(g => new StudentScoreTrendDto
                {
                    Labels = $"Week {g.Key}",
                    Scores = Math.Round(g.Average(s => s.TotalScore), 2)
                })
                .ToList();

            return new BaseResponse<List<StudentScoreTrendDto>>
            {
                Message = "Student score trends generated.",
                Status = true,
                Data = trends
            };
        }


        public async Task<BaseResponse<List<SubmissionDto>>> GetSubmittedAssessmentsAsync()
        {
            var userId = _currentUser.GetCurrentUserId();
            DateTime now = DateTime.UtcNow;
            var submissions = await _submissionRepo.GetStudentSubmissionsAsync(userId , new PaginationRequest { CurrentPage =1,PageSize=5} );

            var submissionDto = submissions.Items.Select( s => new SubmissionDto
            {
                Id = s.Id,
                AssessmentId = s.AssessmentId,
                AssessmentTitle = s.Assessment.Title,
                SubmittedAt = s.SubmittedAt,
                TotalScore = s.TotalScore,
                FeedBack = s.FeedBack
            });
            return new BaseResponse<List<SubmissionDto>>
            {
                Message = "Submitted assessments retrieved successfully",
                Status = true,
                Data = submissionDto.ToList()
            };
        }

        public async Task<BaseResponse<List<UserDto>>> GetInstructors()
        {
            var users = await _userRepository.GetAllAsync(x => x.Role == Role.Instructor);
            if (users is null || !users.Any())
                throw new ApiException("No Instructors found", 404, "NoInstructorsInSystem", null);
            var userDtos = users.Select(x => new UserDto()
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,

            });

            return new BaseResponse<List<UserDto>>()
            {
                Message = "Instructors Retreived",
                Status = true,
                Data = userDtos.ToList()
            };
        }
        public async Task<BaseResponse<StudentProfileMetrics>> GetStudentMetrics()
        {
            var studentId = _currentUser.GetCurrentUserId();
            if (studentId == Guid.Empty)
                throw new ApiException("Invalid student ID provided", 400, "InvalidId", null);
            var student = await _userRepository.GetAsync(studentId);
            if (student == null)
                throw new ApiException("No users found", 404, "UserNotFound", null);
            var submissions = await _submissionRepo.GetAllAsync(x => x.StudentId == studentId);
            if (!submissions.Any())
                throw new ApiException("NoSubmissionForStudent", 500, "UnknownError", null);
            var studentBatchRanking = await _leaderboardStore.GetLeaderBoardByBatchId(student.BatchId.Value);
            var currentStudentRanking = studentBatchRanking.FindIndex(x => x.Id == studentId);
   
            return new BaseResponse<StudentProfileMetrics>
            {
                Message = "Student dashboard summary retrieved successfully",
                Status = true,
                Data = new StudentProfileMetrics
                {
                    SubmmittedCount = submissions.Count,
                    AverageScore = submissions.Average(s => s.TotalScore),
                    PassRate = submissions.Any()
                    ? (double)submissions.Count(x => x.TotalScore >= x.Assessment.RequiredPassingScore)
                        / submissions.Count * 100
                    : 0,
                    Rank = currentStudentRanking + 1

                }
            };
        }
        public async Task<BaseResponse<PaginationDto<SubmissionsDto>>> GetStudentSubmissionsAsync(Guid studentId, PaginationRequest request)
        {

            var check = await _userRepository.CheckAsync(x => x.Id == studentId);
            if (!check)
                throw new ApiException("InvalidStudentId", 404, "Invalid_Id", null);
            var submissions = await _submissionRepo.GetStudentSubmissionsAsync(studentId,request);
            var data = submissions.Items.Select(s => new SubmissionsDto
            {
                Id = s.Id,
                Title = s.Assessment.Title,
                TotalScore = s.TotalScore,
                FeedBack = s.FeedBack,
                AssignedDate = s.Assessment.CreatedAt,
                SubmittedAt = s.SubmittedAt,
                StudentName = s.Student.FullName
            }).ToList();

            var paginationDto = new PaginationDto<SubmissionsDto>()
            {
                TotalItems = submissions.TotalItems,
                TotalPages = submissions.TotalPages,
                CurrentPage = submissions.CurrentPage,
                HasNextPage = submissions.HasNextPage,
                HasPreviousPage = submissions.HasPreviousPage,
                PageSize = submissions.PageSize,
                Items = data
            };
            return new BaseResponse<PaginationDto<SubmissionsDto>>
            {
                Status = true,
                Message = "Student submissions retrieved",
                Data = paginationDto
            };
        }
        public async Task<BaseResponse<UserDto>> UploadStudentFileAsync(UploadFileDto studentFile)
        {
            if (studentFile == null || studentFile.StudentFile == null || studentFile.StudentFile.Length == 0)
                throw new ApiException("No file uploaded.", 400, "FileEmpty", null);

            var batchExists = await _batchRepository.CheckAsync(x => x.Id == studentFile.BatchId);
            if (!batchExists)
                throw new ApiException("Invalid batch ID.", 404, "BatchNotFound", null);

            var students = new List<RegisterUserRequestModel>();

            using var stream = new StreamReader(studentFile.StudentFile.OpenReadStream());
            var content = await stream.ReadToEndAsync();

            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length <= 1)
                throw new ApiException("The uploaded file contains no student data.", 400, "EmptyFile", null);

            var invalidRows = new List<string>();

            foreach (var line in lines.Skip(1)) 
            {
                var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;

                var fullName = parts[0].Trim();
                var email = parts[1].Trim();
                var emailValidator = new EmailAddressAttribute();

                if (!emailValidator.IsValid(email))
                {
                    invalidRows.Add(line); // record the bad line
                    continue;
                }

                if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
                    continue;

                students.Add(new RegisterUserRequestModel
                {
                    FullName = fullName,
                    Email = email,
                    Password = Password, 
                    BatchId = studentFile.BatchId
                });
            }

            if (!students.Any())
                throw new ApiException("No valid student records found in the uploaded file.", 400, "InvalidContent", null);

            var emails = students.Select(x => x.Email).ToList();
            var existingEmails = await _userRepository.CheckEmails(emails);

            if (existingEmails.Any())
                throw new ApiException($"Duplicate emails found: {string.Join(", ", existingEmails)}", 400, "DuplicateEmails", existingEmails);

            var batch = await _batchRepository.GetBatchByIdAsync(studentFile.BatchId);
            if (batch is null)
                throw new ApiException("Batch not found", 404, "BatchNotFound", null);

            var newUsers = students.Select(s => new User
            {
                FullName = s.FullName,
                Email = s.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(s.Password),
                Role = Role.Student,
                BatchId = batch.Id,
                IsActive = true
            }).ToList();

            await _userRepository.CreateAsync(newUsers);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse<UserDto>
            {
                Status = true,
                Message = $"{newUsers.Count} student(s) registered successfully  {invalidRows.Count} row(s) skipped due to invalid data.."
            };
        }
        public async Task<BaseResponse<List<StudentScoreByTypeDto>>> GetScoreByTypeAsync(Guid studentId, DateTime? startDate, DateTime? endDate)
        {
            var submissions = await _submissionRepo.GetAllAsync(s =>
                s.StudentId == studentId &&
                (!startDate.HasValue || s.SubmittedAt.Date >= startDate.Value.Date) &&
                (!endDate.HasValue || s.SubmittedAt.Date <= endDate.Value.Date)
            );

            if (submissions == null || !submissions.Any())
                throw new ApiException("No submissions found for the student in the specified range.", 404, "NoSubmissionsFound", null);

            var allAnswers = submissions
                .SelectMany(s => s.AnswerSubmissions)
                .Where(a => a.Question != null)
                .ToList();

            var grouped = allAnswers
                .GroupBy(a => a.Question.QuestionType.ToString())
                .Select(g =>
                {
                    var totalEarned = g.Sum(x => x.Score);
                    var totalPossible = g.Sum(x => x.Question.Marks);

                    return new StudentScoreByTypeDto
                    {
                        Type = g.Key,
                        AverageScore = totalPossible > 0 ? (double)(totalEarned / totalPossible) * 100 : 0,
                        AttemptCount = g.Count()
                    };
                })
                .ToList();

            return new BaseResponse<List<StudentScoreByTypeDto>>
            {
                Data = grouped,
                Message = "Score by question type generated",
                Status = true
            };
        }
        public async Task<BaseResponse<List<StudentScoreByTypeDto>>> GetScoreByTypeForCurrentStudentAsync(DateTime? startDate, DateTime? endDate)
        {
            var studentId = _currentUser.GetCurrentUserId();
            if (studentId == Guid.Empty)
                throw new ApiException("Invalid student ID", 400, "InvalidStudent", null);

            var submissions = await _submissionRepo.GetAllAsync(s =>
                s.StudentId == studentId &&
                (!startDate.HasValue || s.SubmittedAt.Date >= startDate.Value.Date) &&
                (!endDate.HasValue || s.SubmittedAt.Date <= endDate.Value.Date)
            );

            if (submissions == null || !submissions.Any())
                throw new ApiException("No submissions found for the student in the specified range.", 404, "NoSubmissionsFound", null);

            var allAnswers = submissions
                .SelectMany(s => s.AnswerSubmissions)
                .Where(a => a.Question != null)
                .ToList();

            var grouped = allAnswers
                .GroupBy(a => a.Question.QuestionType.ToString())
                .Select(g =>
                {
                    var totalEarned = g.Sum(x => x.Score);
                    var totalPossible = g.Sum(x => x.Question.Marks);

                    return new StudentScoreByTypeDto
                    {
                        Type = g.Key,
                        AverageScore = totalPossible > 0
                    ? Math.Round(((double)totalEarned / totalPossible) * 100, 2)
                    : 0,
                        AttemptCount = g.Count()
                    };
                })
                .ToList();

            return new BaseResponse<List<StudentScoreByTypeDto>>
            {
                Data = grouped,
                Message = "Score by question type retrieved successfully",
                Status = true
            };
        }
        public async Task<BaseResponse<List<StudentScoreByTypeDto>>> GetScoreByTypeForInstructorAsync(
     Guid? batchId, DateTime? startDate, DateTime? endDate)
        {
            var instructorId = _currentUser.GetCurrentUserId();

            var submissions = await _submissionRepo.GetAllAsync(s =>
                s.Assessment.InstructorId == instructorId &&
                (!batchId.HasValue || s.Student.BatchId == batchId) &&
                (!startDate.HasValue || s.SubmittedAt.Date >= startDate.Value.Date) &&
                (!endDate.HasValue || s.SubmittedAt.Date <= endDate.Value.Date)
            );

            if (!submissions.Any())
            {
                return new BaseResponse<List<StudentScoreByTypeDto>>
                {
                    Status = true,
                    Message = "No submissions found.",
                    Data = new List<StudentScoreByTypeDto>()
                };
            }

            var allAnswers = submissions
                .SelectMany(s => s.AnswerSubmissions)
                .Where(a => a.Question != null)
                .ToList();

            var grouped = allAnswers
                .GroupBy(a => a.Question.QuestionType.ToString())
                .Select(g =>
                {
                    var totalEarned = g.Sum(x => x.Score);
                    var totalPossible = g.Sum(x => x.Question.Marks);

                    return new StudentScoreByTypeDto
                    {
                        Type = g.Key,
                        AverageScore = totalPossible > 0 ? Math.Round((double)(totalEarned / totalPossible) * 100, 2) : 0,
                        AttemptCount = g.Count()
                    };
                })
                .ToList();

            return new BaseResponse<List<StudentScoreByTypeDto>>
            {
                Data = grouped,
                Message = "Score by question type retrieved successfully",
                Status = true
            };
        }





    }
}
