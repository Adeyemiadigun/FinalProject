using System.Linq.Expressions;
using System.Net;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.ExternalServices;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Entitties;
using Domain.Enum;

namespace Application.Services
{
    public class AssessmentService(IUnitOfWork _unitOfWork, IAssessmentRepository _assessmentRepository, IUserRepository _userRepository, IBackgroundService _backgroundService, ICurrentUser _currentUser,IBatchRepository _batchRepository,IBatchService batchService ) : IAssessmentService
    {
                 //case 400: // Bad Request
                 //   case 401: // Unauthorized
                 //   case 403: // Forbidden
                 //   case 404: // Not Found
                 //   case 409: // Conflict
                 //   case 422: // Unprocessable Entity
        public async Task<BaseResponse<AssessmentDto>> AssignStudents(Guid id, AssignStudentsModel model)
        {
            if (id == Guid.Empty || model.StudentIds == null || model.StudentIds.Count == 0)
            {
                throw new ApiException("Invalid input data", (int)HttpStatusCode.BadRequest, "INVALID_INPUT_DATA", null);
            }
            var assessment = await _assessmentRepository.GetAsync(id);
            if (assessment == null)
            {
                throw new ApiException("Assessment not found", (int)HttpStatusCode.NotFound, "ASSESSMENT_NOT_FOUND", null);
            }
            var validStudentIds = await _userRepository.GetAllAsync(x => model.StudentIds.Contains(x.Id) && x.Role == Role.Student);
            if (validStudentIds.Count != model.StudentIds.Count)
            {
                throw new ApiException("Some student IDs are invalid or not students.", (int)HttpStatusCode.BadRequest, "INVALID_STUDENT_IDS", null);
            }
            var alreadyAssignedStudentIds = assessment.AssessmentAssignments.Select(a => a.StudentId).ToHashSet();
            var newAssignments = validStudentIds
                .Where(student => !alreadyAssignedStudentIds.Contains(student.Id))
                .Select(student => new AssessmentAssignment
                {
                    StudentId = student.Id,
                    Student = student,
                    AssessmentId = assessment.Id,
                    Assessment = assessment,
                })
                .ToList();
            foreach (var assignment in newAssignments)
            {
                assessment.AssessmentAssignments.Add(assignment);
            }
            _assessmentRepository.Update(assessment);
            _backgroundService.Enqueue<IEmailService>(emailService => emailService.SendBulkEmailAsync(validStudentIds, "New Assessment", new AssessmentDto()
            {
                Title = assessment.Title,
                Description = assessment.Description,
                TechnologyStack = assessment.TechnologyStack,
                DurationInMinutes = assessment.DurationInMinutes,
                StartDate = assessment.StartDate,
                EndDate = assessment.EndDate,
                PassingScore = assessment.PassingScore
            }));
            var reminderTime = assessment.StartDate.AddMinutes(-30); 
            var delay = reminderTime - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                _backgroundService.Schedule<IEmailService>((emailService => emailService.SendBulkEmailAsync(validStudentIds, "Assessment Reminder", new AssessmentDto() {
                    Title = assessment.Title,
                    Description = assessment.Description,
                    TechnologyStack = assessment.TechnologyStack,
                    DurationInMinutes = assessment.DurationInMinutes,
                    StartDate = assessment.StartDate,
                    EndDate = assessment.EndDate,
                    PassingScore = assessment.PassingScore
                })),delay);
            }
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse<AssessmentDto>()
            {
                Status = true,
                Message = "All Students have been assigned."
            };
        }

        public async Task<BaseResponse<AssessmentDto>> CreateAssessmentAsync(CreateAssessmentRequestModel model)
        {
            var currentUser = _currentUser.GetCurrentUserId();
            if (currentUser == Guid.Empty)
                throw new ApiException("Current user ID is invalid", (int)HttpStatusCode.BadRequest, "INVALID_USER_ID", null);

            var user = await _userRepository.GetAsync(currentUser) ?? throw new ApiException("User not found", (int)HttpStatusCode.NotFound, "USER_NOT_FOUND", null);

            var batches = await _batchRepository.GetSelectedIds(model.BatchIds);
            if (batches.Count == 0)
                throw new ApiException("No valid batches found", (int)HttpStatusCode.BadRequest, "NO_VALID_BATCHES", null);
            if(batches.Count != model.BatchIds.Count)
                throw new ApiException("Some batch IDs are invalid", (int)HttpStatusCode.BadRequest, "INVALID_BATCH_IDS", null);
            var assessment = new Assessment()
            {
                Title = model.Title,
                Description = model.Description,
                TechnologyStack = model.TechnologyStack,
                DurationInMinutes = model.DurationInMinutes,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                PassingScore = model.PassingScore,
                InstructorId = user.Id,
                Instructor = user,
            };

            var batchAssessment = batches.Select(x => new BatchAssessment()
            {
                BatchId = x.Id,
                Batch = x,
                AssessmentId = assessment.Id,
                Assessment = assessment
            });
            foreach(var batch in batchAssessment)
            {

                assessment.BatchAssessment.Add(batch);
            }
            foreach(var batch in batches)
            {

                await batchService.AssignAssessmentToBatchAsync(batch.Id, assessment);
            }

            await _unitOfWork.SaveChangesAsync();
            

            return new BaseResponse<AssessmentDto>()
            {
                Status = true,
                Message = "Assessment created successfully",
                Data = new AssessmentDto()
                {
                    Title = assessment.Title,
                    Description = assessment.Description,
                    TechnologyStack = assessment.TechnologyStack,
                    DurationInMinutes = assessment.DurationInMinutes,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    PassingScore = model.PassingScore
                }
            };
        }

        public Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAllAssessmentsAsync(PaginationRequest request)
        {
            throw new NotImplementedException();
        }

      

        public async Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAllAssessmentsByInstructorIdAsync(PaginationRequest request, Guid instructorId = default)
        {
            var userId = instructorId == default ? _currentUser.GetCurrentUserId() : instructorId;
            if (userId == Guid.Empty)
            {
                throw new ApiException("User ID is required", (int)HttpStatusCode.BadRequest, "USER_ID_REQUIRED", null);
            }
            var assessments = await _assessmentRepository.GetAllAsync(x => x.InstructorId == userId, request);
            var paginationDto = new PaginationDto<AssessmentDto>
            {
                Items = assessments.Items.Select(x => new AssessmentDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    TechnologyStack = x.TechnologyStack,
                    DurationInMinutes = x.DurationInMinutes,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    PassingScore = x.PassingScore
                }).ToList(),
                TotalItems = assessments.TotalItems, // Corrected property name
                CurrentPage = assessments.CurrentPage,
                PageSize = assessments.PageSize,
                TotalPages = assessments.TotalPages,
                HasNextPage = assessments.HasNextPage,
                HasPreviousPage = assessments.HasPreviousPage
            };
            return new BaseResponse<PaginationDto<AssessmentDto>>()
            {
                Status = true,
                Message = "Assessments retrieved successfully",
                Data = paginationDto
            };
        }
       public async Task<BaseResponse<PaginationDto<AssessmentDto>>> GetCurrentStudentAssessments(
    PaginationRequest request, string status)
        {
            var userId = _currentUser.GetCurrentUserId();
            if (userId == Guid.Empty)
                throw new ApiException("User ID is required", (int)HttpStatusCode.BadRequest, "USER_ID_REQUIRED", null);

            DateTime now = DateTime.UtcNow;

            // Build the complete filter with status included directly
            Expression<Func<Assessment, bool>> filter = x =>
                x.AssessmentAssignments.Any(a => a.StudentId == userId) &&
                (
                    string.IsNullOrWhiteSpace(status) ||
                    (status.ToLower() == "upcoming" && x.StartDate > now) ||
                    (status.ToLower() == "inprogress" && x.StartDate <= now && x.EndDate >= now) ||
                    (status.ToLower() == "completed" && x.EndDate < now)
                );

            var assessments = await _assessmentRepository.GetAllAsync(filter, request);

            var paginationDto = new PaginationDto<AssessmentDto>
            {
                Items = assessments.Items.Select(x => new AssessmentDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    TechnologyStack = x.TechnologyStack,
                    DurationInMinutes = x.DurationInMinutes,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    PassingScore = x.PassingScore,
                    Submitted = x.Submissions.Any()
                }).ToList(),
                TotalItems = assessments.TotalItems,
                CurrentPage = assessments.CurrentPage,
                PageSize = assessments.PageSize,
                TotalPages = assessments.TotalPages,
                HasNextPage = assessments.HasNextPage,
                HasPreviousPage = assessments.HasPreviousPage
            };

            return new BaseResponse<PaginationDto<AssessmentDto>>
            {
                Status = true,
                Message = "Assessments retrieved successfully",
                Data = paginationDto
            };
        }

   

        public async Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAssessmentsByStudentId(Guid studentId, PaginationRequest request)
        {
            if (studentId == Guid.Empty)
            {
                throw new ApiException("User ID is required", (int)HttpStatusCode.BadRequest, "USER_ID_REQUIRED", null);
            }
            return await GetAllStudentAssessments(request, studentId);
        }
        public async Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAllStudentAssessments(PaginationRequest request, Guid studentId)
        {
           
            var assessments = await _assessmentRepository.GetAllAsync(
                x => x.AssessmentAssignments.Any(a => a.StudentId == studentId),
                request
            );

            var paginationDto = new PaginationDto<AssessmentDto>
            {
                Items = assessments.Items.Select(x => new AssessmentDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    TechnologyStack = x.TechnologyStack,
                    DurationInMinutes = x.DurationInMinutes,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    PassingScore = x.PassingScore
                }).ToList(),
                TotalItems = assessments.TotalItems,
                CurrentPage = assessments.CurrentPage,
                PageSize = assessments.PageSize,
                TotalPages = assessments.TotalPages,
                HasNextPage = assessments.HasNextPage,
                HasPreviousPage = assessments.HasPreviousPage
            };

            return new BaseResponse<PaginationDto<AssessmentDto>>()
            {
                Status = true,
                Message = "Assessments retrieved successfully",
                Data = paginationDto
            };
        }

        public async Task<BaseResponse<AssessmentDto>> GetAssessmentAsync(Guid id)
        {
            var assessment = await _assessmentRepository.GetAsync(id)
                ?? throw new ApiException("Assessment not found", (int)HttpStatusCode.NotFound, "ASSESSMENT_NOT_FOUND", null);

            var assessmentDto = new AssessmentDto
            {
                Id = assessment.Id,
                Title = assessment.Title,
                Description = assessment.Description,
                TechnologyStack = assessment.TechnologyStack,
                DurationInMinutes = assessment.DurationInMinutes,
                StartDate = assessment.StartDate,
                EndDate = assessment.EndDate,
                PassingScore = assessment.PassingScore
            };

            return new BaseResponse<AssessmentDto>
            {
                Status = true,
                Message = "Assessment retrieved successfully",
                Data = assessmentDto
            };
        }

        public async Task<BaseResponse<AssessmentDto>> UpdateAssessmentAsync(Guid id, UpdateAssessmentRequestModel model)
        {
            if (id == Guid.Empty || model == null)
            {
                throw new ApiException("Invalid input data", 400, "InvalidInputData", null);
            }
            var currentUserId = _currentUser.GetCurrentUserId();
            var assessment = await _assessmentRepository.GetAsync(id);

            if (assessment == null)
                throw new ApiException("Assessment not found", 404, "AssessmentNotFound", null);

            if (assessment.InstructorId != currentUserId)
                throw new ApiException("Unauthorized: You can only update your own assessments", 403, "UnauthorizedAccess", null);
            assessment.Title = model.Title;
            assessment.Description = model.Description;
            assessment.TechnologyStack = model.TechnologyStack;
            assessment.DurationInMinutes = model.DurationInMinutes;
            assessment.StartDate = model.StartDate;
            assessment.EndDate = model.EndDate;
            assessment.PassingScore = model.PassingScore;
            _assessmentRepository.Update(assessment);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse<AssessmentDto>()
            {
                Status = true,
                Message = "Assessment updated successfully",
                Data = new AssessmentDto()
                {
                    Id = assessment.Id,
                    Title = assessment.Title,
                    Description = assessment.Description,
                    TechnologyStack = assessment.TechnologyStack,
                    DurationInMinutes = assessment.DurationInMinutes,
                    StartDate = assessment.StartDate,
                    EndDate = assessment.EndDate,
                    PassingScore = assessment.PassingScore
                }
            };
        }

        public async Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAssessmentsByBatchId(Guid id,PaginationRequest request)
        {
            if (id == Guid.Empty)
            {
                throw new ApiException("Batch ID is required", (int)HttpStatusCode.BadRequest, "BATCH_ID_REQUIRED", null);
            }
            var batch = await _batchRepository.GetBatchByIdAsync(id);
            if (batch == null)
            {
                throw new ApiException("Batch not found", (int)HttpStatusCode.NotFound, "BATCH_NOT_FOUND", null);
            }

            var assessments = await _assessmentRepository.GetAllAsync(x => x.BatchAssessment.Any(x => x.BatchId == id), request);
            if (assessments == null || !assessments.Items.Any())
            {
                throw new ApiException("No assessments found for the given batch ID", (int)HttpStatusCode.NotFound, "ASSESSMENTS_NOT_FOUND", null);
            }

            var assessmentDtos = assessments.Items.Select(x => new AssessmentDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                TechnologyStack = x.TechnologyStack,
                DurationInMinutes = x.DurationInMinutes,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                PassingScore = x.PassingScore
            }).ToList();
            var paginationDto = new PaginationDto<AssessmentDto>()
            {
                Items = assessmentDtos,
                TotalItems = assessments.TotalItems,
                CurrentPage = assessments.CurrentPage,
                PageSize = assessments.PageSize,
                TotalPages = assessments.TotalPages,
                HasNextPage = assessments.HasNextPage,
                HasPreviousPage = assessments.HasPreviousPage
            };

            return new BaseResponse<PaginationDto<AssessmentDto>>
            {
                Status = true,
                Message = "Assessments retrieved successfully",
                Data = paginationDto
            };
        }
        public async Task<BaseResponse<PaginationDto<BatchAssessmentsOverview>>> GetAssessmentsByBatchIdDetails(Guid id, PaginationRequest request)
        {
            if (id == Guid.Empty)
            {
                throw new ApiException("Batch ID is required", (int)HttpStatusCode.BadRequest, "BATCH_ID_REQUIRED", null);
            }
            var batch = await _batchRepository.GetBatchByIdAsync(id);
            if (batch == null)
            {
                throw new ApiException("Batch not found", (int)HttpStatusCode.NotFound, "BATCH_NOT_FOUND", null);
            }

            var assessments = await _assessmentRepository.GetAllAsync(x => x.BatchAssessment.Any(x => x.BatchId == id), request);
            if (assessments == null || !assessments.Items.Any())
            {
                throw new ApiException("No assessments found for the given batch ID", (int)HttpStatusCode.NotFound, "ASSESSMENTS_NOT_FOUND", null);
            }

            var assessmentDtos = assessments.Items.Select(x => new BatchAssessmentsOverview
            {
                Id = x.Id,
                Title = x.Title,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                TotalStudents = x.AssessmentAssignments.Count,
                Submissions = x.Submissions.Count,
                AvgScore = x.Submissions.Any() ? Math.Round(x.Submissions.Average(s => s.TotalScore), 2) : 0
            }).ToList();
            var paginationDto = new PaginationDto<BatchAssessmentsOverview>()
            {
                Items = assessmentDtos,
                TotalItems = assessments.TotalItems,
                CurrentPage = assessments.CurrentPage,
                PageSize = assessments.PageSize,
                TotalPages = assessments.TotalPages,
                HasNextPage = assessments.HasNextPage,
                HasPreviousPage = assessments.HasPreviousPage
            };

            return new BaseResponse<PaginationDto<BatchAssessmentsOverview>>
            {
                Status = true,
                Message = "Assessments retrieved successfully",
                Data = paginationDto
            };
        }

        public async Task<BaseResponse<List<AssessmentPerformanceDto>>> GetTopAssessments()
        {
            var assessments = await _assessmentRepository .GetAllAsync(x => x.AssessmentAssignments.Any(), new PaginationRequest { PageSize = 3, CurrentPage = 1 });
            if (assessments == null || !assessments.Items.Any())
            {
                throw new ApiException("No assessments found", (int)HttpStatusCode.NotFound, "ASSESSMENTS_NOT_FOUND", null);
            }
            var assessmentPerformance = assessments.Items.Select(x => new AssessmentPerformanceDto
            {
                Id = x.Id,
                AssessmentTitle = x.Title,
                AverageScore = x.Submissions.Any() ? x.Submissions.Average(s => s.TotalScore) : 0,
            }).ToList();
            return new BaseResponse<List<AssessmentPerformanceDto>>
            {
                Status = true,
                Message = "Top assessments retrieved successfully",
               Data = assessmentPerformance
            };
        }

        public async Task<BaseResponse<List<AssessmentPerformanceDto>>> GetLowestAssessments()
        {
            var assessments = await _assessmentRepository.GetAllAsync(
                x => x.AssessmentAssignments.Any(),
                new PaginationRequest { PageSize = 3, CurrentPage = 1 }
            );
            if(assessments == null || !assessments.Items.Any())
            {
                throw new ApiException("No assessments found", (int)HttpStatusCode.NotFound, "ASSESSMENTS_NOT_FOUND", null);
            }
            var assessmentPerformance = assessments.Items
                .Select(x => new AssessmentPerformanceDto
                {
                    Id = x.Id,
                    AssessmentTitle = x.Title,
                    AverageScore = x.Submissions.Any() ? x.Submissions.Average(s => s.TotalScore) : 0,
                })
                .OrderBy(x => x.AverageScore) // Sort by lowest scores
                .ToList();

            return new BaseResponse<List<AssessmentPerformanceDto>>
            {
                Status = true,
                Message = "Lowest assessments retrieved successfully",
                Data = assessmentPerformance
            };
        }

        public async Task<BaseResponse<List<AssessmentDto>>> GetRecentAssessment()
        {
            var recentAssessments = await _assessmentRepository.GetAllAsync(
                x => x.CreatedAt >= DateTime.UtcNow.AddDays(-7),
                new PaginationRequest { PageSize = 5, CurrentPage = 1 } 
            );

            if (recentAssessments == null || !recentAssessments.Items.Any())
            {
                return new BaseResponse<List<AssessmentDto>>
                {
                    Status = false,
                    Message = "No recent assessments found",
                    Data = null
                };
            }

            var assessmentDtos = recentAssessments.Items.Select(x => new AssessmentDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                TechnologyStack = x.TechnologyStack,
                DurationInMinutes = x.DurationInMinutes,
                InstructorName = x.Instructor?.FullName ?? "Unknown Instructor",
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                CreatedAt = x.CreatedAt,
                PassingScore = x.PassingScore
            }).ToList();

            return new BaseResponse<List<AssessmentDto>>
            {
                Status = true,
                Message = "Recent assessments retrieved successfully",
                Data = assessmentDtos
            };
        }
        public async Task<BaseResponse<List<AssessmentDto>>> GetInstructorRecentAssessment()
        {
            var userId = _currentUser.GetCurrentUserId();
            if(userId == Guid.Empty)
            {
                throw new ApiException("Current user ID is invalid", (int)HttpStatusCode.BadRequest, "INVALID_USER_ID", null);
            }
            var recentAssessments = await _assessmentRepository.GetAllAsync(
                x => x.CreatedAt >= DateTime.UtcNow.AddDays(-7) && x.InstructorId == userId ,
                new PaginationRequest { PageSize = 5, CurrentPage = 1 }
            );

            if (recentAssessments == null || !recentAssessments.Items.Any())
            {
                return new BaseResponse<List<AssessmentDto>>
                {
                    Status = false,
                    Message = "No recent assessments found",
                    Data = null
                };
            }

            var assessmentDtos = recentAssessments.Items.Select(x => new AssessmentDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                TechnologyStack = x.TechnologyStack,
                DurationInMinutes = x.DurationInMinutes,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                CreatedAt = x.CreatedAt,
                PassingScore = x.PassingScore
            }).ToList();

            return new BaseResponse<List<AssessmentDto>>
            {
                Status = true,
                Message = "Recent assessments retrieved successfully",
                Data = assessmentDtos
            };
        }
        public async Task<BaseResponse<List<AssessmentPerformanceDto>>> GetInstructorAssessmentScoresAsync()
        {
            var currentUserId = _currentUser.GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                throw new ApiException("Current user ID is invalid", (int)HttpStatusCode.BadRequest, "INVALID_USER_ID", null);
            }
            var check = await _userRepository.CheckAsync(x => x.Id == currentUserId);
            if (!check)
            {
                throw new ApiException("User not found", (int)HttpStatusCode.NotFound, "USER_NOT_FOUND", null);
            }
            var assessments = await _assessmentRepository.GetAllAsync(x => x.InstructorId == currentUserId);
            var assessmentScores = assessments
                .Select(x => new AssessmentPerformanceDto
                {
                    Id = x.Id,
                    AssessmentTitle = x.Title,
                    AverageScore = x.Submissions.Any() ? x.Submissions.Average(s => s.TotalScore) : 0,
                })
                .ToList();
            return new BaseResponse<List<AssessmentPerformanceDto>>
            {
                Status = true,
                Message = "Assessment scores retrieved successfully",
                Data = assessmentScores
            };
        }
        public async Task<BaseResponse<PaginationDto<InstructorAssessmentDto>>> GetAssessmentsByInstructorAsync(Guid? batchId, string? status, PaginationRequest request)
        {
            var userId = _currentUser.GetCurrentUserId();
            var now = DateTime.UtcNow;

            var assessments = await _assessmentRepository.GetAllAsync(x =>
                x.InstructorId == userId &&
                (!batchId.HasValue || x.BatchAssessment.Any(b => b.BatchId == batchId)) &&
                (string.IsNullOrEmpty(status) ||
                    (status == "upcoming" && x.StartDate > now) ||
                    (status == "past" && x.EndDate < now)),request);

            var items = assessments.Items.Select(a => new InstructorAssessmentDto
            {
                Id = a.Id,
                Title = a.Title,
                TechnologyStack = a.TechnologyStack,
                DurationInMinutes = a.DurationInMinutes,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                CreatedAt = a.CreatedAt,
                BatchNames = a.BatchAssessment.Select(b => $"{b.Batch.Name} {b.Batch.BatchNumber}").ToList(),
                Students = a.Submissions.Select(s => new StudentScoreDto
                {
                    StudentId = s.Student.Id,
                    Name = s.Student.FullName,
                    Email = s.Student.Email,
                    Batch = s.Student.Batch.Name,
                    Score = s.TotalScore
                }).ToList(),

                BatchPerformance = a.Submissions
                    .GroupBy(s => s.Student.Batch.Name)
                    .Select(g => new BatchPerformanceDto
                    {
                        BatchName = g.Key,
                        AverageScore = Math.Round(g.Average(s => s.TotalScore), 2)
                    }).ToList()
            }).ToList();

            var pagination = new PaginationDto<InstructorAssessmentDto>()
            {
                TotalItems = assessments.TotalItems,
                TotalPages = assessments.TotalPages,
                CurrentPage = assessments.CurrentPage,
                PageSize = assessments.PageSize,
                HasNextPage = assessments.HasNextPage,
                HasPreviousPage = assessments.HasPreviousPage,
                Items = items

            };
            return new BaseResponse<PaginationDto<InstructorAssessmentDto>>()
            {
                Message = "",
                Data = pagination,
                Status = true
            };
        }
        public async Task<BaseResponse<PaginationDto<StudentAssessmentDetail>>> GetStudentAssessmentDetails(Guid id, PaginationRequest request)
        {
            if (Guid.Empty == id)
            {
                throw new ApiException("Assessment ID is required", (int)HttpStatusCode.BadRequest, "ASSESSMENT_ID_REQUIRED", null);
            }
            var assessment = await _assessmentRepository.GetAllAsync(x => x.AssessmentAssignments.Any(b => b.StudentId == id), request);
            if (assessment is null)
            {
                throw new ApiException("Assessment not found", (int)HttpStatusCode.NotFound, "ASSESSMENT_NOT_FOUND", null);
            }

            var assessments = assessment.Items.Select(x => new StudentAssessmentDetail()
            {
                Id = x.Id,
                Title = x.Title,
                Score = x.Submissions.Any(s => s.StudentId == id) ? x.Submissions.FirstOrDefault(s => s.StudentId == id)!.TotalScore : 0,
                Status = x.Submissions.Any(s => s.StudentId == id),
                AssignedDate = x.StartDate,
                SubmittedDate = x.Submissions.FirstOrDefault(s => s.StudentId == id)?.SubmittedAt
            }).ToList();
            var paginatedResult = new PaginationDto<StudentAssessmentDetail>()
            {
                TotalItems = assessment.TotalItems,
                TotalPages = assessment.TotalPages,
                HasNextPage = assessment.HasNextPage,
                HasPreviousPage = assessment.HasPreviousPage,
                CurrentPage = assessment.CurrentPage,
                PageSize = assessment.PageSize,
                Items = assessments
            };
            return new BaseResponse<PaginationDto<StudentAssessmentDetail>>()
            {
                Message = "Assessment details retrieved successfully",
                Status = true,
                Data = paginatedResult
            };
        }
        public async Task<BaseResponse<PaginationDto<InstructorAssessmentPerformanceDetailDto>>> GetInstructorAssessmentDetail(Guid instructorId, PaginationRequest request)
        {
            if (instructorId == Guid.Empty)
            {
                throw new ApiException("Instructor ID is required", (int)HttpStatusCode.BadRequest, "INSTRUCTOR_ID_REQUIRED", null);
            }

            var assessment = await _assessmentRepository.GetAllAsync(x => x.InstructorId == instructorId, request);
            var instructorAssessmentDetail = assessment.Items.Select(x => new InstructorAssessmentPerformanceDetailDto()
            {
                Id = x.Id,
                Title = x.Title,
                StudentCount = x.AssessmentAssignments.Count,
                AvgScore = x.Submissions.Any() ? Math.Round(x.Submissions.Average(s => s.TotalScore), 2) : 0,
                CreatedAt = x.CreatedAt
            }).ToList();

            var paginatedResponse = new PaginationDto<InstructorAssessmentPerformanceDetailDto>()
            {
                TotalItems = assessment.TotalItems,
                TotalPages = assessment.TotalPages,
                CurrentPage = assessment.CurrentPage,
                PageSize = assessment.PageSize,
                HasNextPage = assessment.HasNextPage,
                HasPreviousPage = assessment.HasPreviousPage,
                Items = instructorAssessmentDetail
            };

            return new BaseResponse<PaginationDto<InstructorAssessmentPerformanceDetailDto>>()
            {
                Message = "Instructor assessment details retrieved successfully",
                Status = true,
                Data = paginatedResponse
            };
        }


    }

}
