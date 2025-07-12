using System.Linq;
using System.Linq.Expressions;
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
        private readonly ICurrentUser _currentUser;
        private readonly ISubmissionRepository _submissionRepo;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly ILeaderboardStore _leaderboardStore;

        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, IBatchRepository batchRepository, ICurrentUser currentUser, ISubmissionRepository submissionRepo, IAssessmentRepository assessmentRepository, ILeaderboardStore leaderboardStore)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _batchRepository = batchRepository;
            _currentUser = currentUser;
            _submissionRepo = submissionRepo;
            _assessmentRepository = assessmentRepository;
            _leaderboardStore = leaderboardStore;
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


        public async Task<BaseResponse<PaginationDto<UserDto>>> SearchByNameOrEmailAsync(string query, PaginationRequest request, string? status = null)
        {
            var normalizedQuery = query.Trim().ToLower();

            var users = await _userRepository.GetAllAsync(s =>
                (s.Role == Role.Student) &&
                (string.IsNullOrEmpty(normalizedQuery) ||
                 s.FullName.ToLower().Contains(normalizedQuery) ||
                 s.Email.ToLower().Contains(normalizedQuery)) &&
                (status == null || (status == "active" && s.IsActive) || (status == "inactive" && !s.IsActive)),
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

            return new BaseResponse<PaginationDto<UserDto>>()
            {
                Message = "Users retrieved successfully",
                Status = true,
                Data = paginationDto
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
            
            List<Guid> assignedAssessmentIds = new();
            if (batchId.HasValue)
            {
                var batch = await _batchRepository.GetBatchByIdAsync(batchId.Value);
                if (batch == null)
                {
                    return new BaseResponse<PaginationDto<LeaderboardDto>>
                    {
                        Status = false,
                        Message = "Batch not found",
                        Data = null
                    };
                }

                assignedAssessmentIds = batch.AssessmentAssignments.Select(x => x.AssessmentId).ToList();
            }

            
            var students = await _userRepository.GetAllWithRelationshipAsync(
                s => !batchId.HasValue || s.BatchId == batchId, request
            );

            if (students.Items == null || !students.Items.Any())
            {
                return new BaseResponse<PaginationDto<LeaderboardDto>>
                {
                    Message = "No students found",
                    Status = true,
                    Data = new PaginationDto<LeaderboardDto>
                    {
                        Items = new List<LeaderboardDto>(),
                        TotalItems = 0,
                        TotalPages = 0,
                        CurrentPage = 0,
                        PageSize = request.PageSize,
                        HasNextPage = false,
                        HasPreviousPage = false
                    }
                };
            }

            
            var leaderboard = students.Items.Select(student =>
            {
                var studentSubmissions = student.Submissions
                    .Where(sub => !batchId.HasValue || assignedAssessmentIds.Contains(sub.AssessmentId))
                    .ToList();

                var completed = studentSubmissions.Count;
                var avgScore = completed > 0 ? Math.Round(studentSubmissions.Average(s => s.TotalScore), 2) : 0;
                var highestScore = completed > 0 ? studentSubmissions.Max(s => s.TotalScore) : 0;

                return new LeaderboardDto
                {
                    Id = student.Id,
                    Name = student.FullName,
                    Batch = student.Batch?.Name ?? "-",
                    CompletedAssessments = completed,
                    AvgScore = avgScore,
                    HighestScore = highestScore,
                };
            })
            .OrderByDescending(l => l.AvgScore)
            .ToList();

           
            return new BaseResponse<PaginationDto<LeaderboardDto>>
            {
                Message = "Leaderboard retrieved successfully",
                Status = true,
                Data = new PaginationDto<LeaderboardDto>
                {
                    TotalItems = students.TotalItems,
                    TotalPages = students.TotalPages,
                    CurrentPage = students.CurrentPage,
                    HasNextPage = students.HasNextPage,
                    HasPreviousPage = students.HasPreviousPage,
                    PageSize = students.PageSize,
                    Items = leaderboard
                }
            };
        }
        public async Task<BaseResponse<PaginationDto<LeaderboardDto>>> GetStudentBatchLeaderboardAsync(PaginationRequest request)
        {
            var userId = _currentUser.GetCurrentUserId();

            // Step 1: Get the current student and their batch
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

            // Step 2: Try to get all leaderboard entries from cache
            var allLeaderboards = await _leaderboardStore.GetLeaderBoardByBatchId(batchId);

            // Step 4: If batch leaderboard is empty, regenerate it
            if (!allLeaderboards.Any())
            {
                var assignedAssessmentIds = student.Batch.AssessmentAssignments
                    .Select(x => x.AssessmentId)
                    .ToList();

                var batchStudents = await _userRepository.GetAllAsync(s => s.BatchId == batchId);

                allLeaderboards = batchStudents.Select(s =>
                {
                    var submissions = s.Submissions
                        .Where(sub => assignedAssessmentIds.Contains(sub.AssessmentId))
                        .ToList();

                    var avgScore = submissions.Any() ? Math.Round(submissions.Average(sub => sub.TotalScore), 2) : 0;
                    var highestScore = submissions.Any() ? submissions.Max(sub => sub.TotalScore) : 0;

                    return new LeaderboardDto
                    {
                        Id = s.Id,
                        Name = s.FullName,
                        Batch = batchId.ToString(), // store batch as Guid string
                        AvgScore = avgScore,
                        HighestScore = highestScore,
                        CompletedAssessments = submissions.Count
                    };
                })
                .OrderByDescending(l => l.AvgScore)
                .ThenByDescending(l => l.HighestScore)
                .ToList();

                // Step 5: Store regenerated leaderboard in the cache
                await _leaderboardStore.StoreLeaderboard(allLeaderboards);
            }

            // Step 6: Paginate the result
            var skip = (request.CurrentPage - 1) * request.PageSize;
            var paginatedItems = allLeaderboards.Skip(skip).Take(request.PageSize).ToList();

            var paginatedResponse = new PaginationDto<LeaderboardDto>
            {
                Items = paginatedItems,
                TotalItems = allLeaderboards.Count,
                TotalPages = (int)Math.Ceiling((double)allLeaderboards.Count / request.PageSize),
                CurrentPage = request.CurrentPage,
                PageSize = request.PageSize,
                HasNextPage = skip + request.PageSize < allLeaderboards.Count,
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
                AverageScore = instructor.Assessments
                    .Where(x => x.Submissions.Any())
                    .SelectMany(x => x.Submissions)
                    .Average(s => s.TotalScore) 
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
            return new BaseResponse<StudentDashboardSummaryDto>
            {
                Message = "Student dashboard summary retrieved successfully",
                Status = true,
                Data = new StudentDashboardSummaryDto
                {
                    TotalAssessments = submissions.Count,
                    AverageScore = submissions.Average(s => s.TotalScore),
                    HighestScore = submissions.Max(s => s.TotalScore),
                    CompletionRate = 100 * submissions.Count(s => s.SubmittedAt != null) / submissions.Count
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
            var assessments = await _assessmentRepository.GetAllAsync(x => x.AssessmentAssignments.Any(x => x.StudentId == studentId)&& x != null && x.StartDate <= DateTime.UtcNow && x.EndDate >= DateTime.UtcNow, new PaginationRequest { CurrentPage = 1, PageSize = 4});

            if (!assessments.Items.Any())
                throw new ApiException("No ongoing assessments found for the student", 404, "NoOngoingAssessments", null);
            var ongoingAssessments = assessments.Items
                .Select(x => new StudentAssessmentDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    StartedDate = x.StartDate,
                    EndDate = x.EndDate,
                    Status = "Ongoing",
                    Type = x.TechnologyStack,
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
                   Status = "Ongoing",
                   Type = x.TechnologyStack,
                   Duration = x.DurationInMinutes
               }).ToList();
            return new BaseResponse<List<StudentAssessmentDto>>
            {
                Message = "Upcoming assessments retrieved successfully",
                Status = true,
                Data = upcomingAssessment
            };
        }

        public async Task<BaseResponse<ScoreTrendDto>> GetScoreTrendAsync()
        {
            var submissions = await _submissionRepo.GetByStudentIdAsync(studentId);
            var grouped = submissions.GroupBy(s => s.SubmittedAt.Date).OrderBy(g => g.Key);

            return new ScoreTrendDto
            {
                Labels = grouped.Select(g => g.Key.ToString("MMM dd")).ToList(),
                Scores = grouped.Select(g => g.Average(x => x.TotalScore)).ToList()
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

        public async Task<BaseResponse<List<StudentRankingDto>>> GetBatchRankingAsync( )
        {
            var student = await _userRepo.GetByIdAsync(studentId);
            var batchStudents = await _userRepo.GetByBatchIdAsync(student.BatchId);
            var ranked = batchStudents
                .Where(s => s.Submissions.Any())
                .Select(s => new StudentRankingDto
                {
                    Id = s.Id,
                    Name = s.FullName,
                    Score = s.Submissions.Average(x => x.TotalScore)
                })
                .OrderByDescending(x => x.Score)
                .ToList();

            for (int i = 0; i < ranked.Count; i++) ranked[i].Rank = i + 1;
            return ranked;
        }
    }
}
