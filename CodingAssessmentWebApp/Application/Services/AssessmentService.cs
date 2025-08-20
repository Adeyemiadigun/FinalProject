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
    public class AssessmentService(IUnitOfWork _unitOfWork, IAssessmentRepository _assessmentRepository, IUserRepository _userRepository, IBackgroundService _backgroundService, ICurrentUser _currentUser, IBatchRepository _batchRepository, IBatchService batchService, IReminderService reminderService) : IAssessmentService
    {
        //case 400: // Bad Request
        //   case 401: // Unauthorized
        //   case 403: // Forbidden
        //   case 404: // Not Found
        //   case 409: // Conflict
        //   case 422: // Unprocessable Entity
        //public async Task<BaseResponse<AssessmentDto>> AssignStudents(Guid id, AssignStudentsModel model)
        //{
        //    if (id == Guid.Empty || model.StudentIds == null || model.StudentIds.Count == 0)
        //    {
        //        throw new ApiException("Invalid input data", (int)HttpStatusCode.BadRequest, "INVALID_INPUT_DATA", null);
        //    }
        //    var assessment = await _assessmentRepository.GetAsync(id);
        //    if (assessment == null)
        //    {
        //        throw new ApiException("Assessment not found", (int)HttpStatusCode.NotFound, "ASSESSMENT_NOT_FOUND", null);
        //    }
        //    var validStudentIds = await _userRepository.GetAllAsync(x => model.StudentIds.Contains(x.Id) && x.Role == Role.Student);
        //    if (validStudentIds.Count != model.StudentIds.Count)
        //    {
        //        throw new ApiException("Some student IDs are invalid or not students.", (int)HttpStatusCode.BadRequest, "INVALID_STUDENT_IDS", null);
        //    }
        //    var alreadyAssignedStudentIds = assessment.AssessmentAssignments.Select(a => a.StudentId).ToHashSet();
        //    var newAssignments = validStudentIds
        //        .Where(student => !alreadyAssignedStudentIds.Contains(student.Id))
        //        .Select(student => new AssessmentAssignment
        //        {
        //            StudentId = student.Id,
        //            Student = student,
        //            AssessmentId = assessment.Id,
        //            Assessment = assessment,
        //        })
        //        .ToList();
        //    foreach (var assignment in newAssignments)
        //    {
        //        assessment.AssessmentAssignments.Add(assignment);
        //    }
        //    _assessmentRepository.Update(assessment);
        //    var validstudentDto = validStudentIds.Select(x => new UserDto
        //    {
        //        Email = x.Email,
        //        FullName = x.FullName
        //    }).ToList();
        //    _backgroundService.Enqueue<IEmailService>(emailService => emailService.SendAssessmentEmail(validstudentDto, "New Assessment", new AssessmentDto()
        //    {
        //        Title = assessment.Title,
        //        Description = assessment.Description,
        //        TechnologyStack = assessment.TechnologyStack,
        //        DurationInMinutes = assessment.DurationInMinutes,
        //        StartDate = assessment.StartDate,
        //        EndDate = assessment.EndDate,
        //        PassingScore = assessment.PassingScore
        //    }));
        //    var reminderTime = assessment.StartDate.AddMinutes(-30);
        //    var delay = reminderTime - DateTime.UtcNow;
        //    if (delay > TimeSpan.Zero)
        //    {
        //        _backgroundService.Schedule<IEmailService>((emailService => emailService.SendBulkEmailAsync(validstudentDto, "Assessment Reminder"), delay);
        //    }
        //    await _unitOfWork.SaveChangesAsync();
        //    return new BaseResponse<AssessmentDto>()
        //    {
        //        Status = true,
        //        Message = "All Students have been assigned."
        //    };
        //}

        public async Task<BaseResponse<AssessmentDto>> CreateAssessmentAsync(CreateAssessmentRequestModel model)
        {
            var currentUser = _currentUser.GetCurrentUserId();
            if (currentUser == Guid.Empty)
                throw new ApiException("Current user ID is invalid", (int)HttpStatusCode.BadRequest, "INVALID_USER_ID", null);

            var user = await _userRepository.GetAsync(currentUser) ?? throw new ApiException("User not found", (int)HttpStatusCode.NotFound, "USER_NOT_FOUND", null);

            var batches = await _batchRepository.GetSelectedIds(model.BatchIds);
            if (batches.Count == 0)
                throw new ApiException("No valid batches found", (int)HttpStatusCode.BadRequest, "NO_VALID_BATCHES", null);
            if (batches.Count != model.BatchIds.Count)
                throw new ApiException("Some batch IDs are invalid", (int)HttpStatusCode.BadRequest, "INVALID_BATCH_IDS", null);
            var assessment = new Assessment()
            {
                Title = model.Title,
                Description = model.Description,
                TechnologyStack = model.TechnologyStack,
                DurationInMinutes = model.DurationInMinutes,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Status = AssessmentStatus.Upcoming,
                PassingScore = model.PassingScore,
                InstructorId = user.Id,
                Instructor = user,
            };

            await _assessmentRepository.CreateAsync(assessment);

            var batchAssessment = batches.Select(x => new BatchAssessment()
            {
                BatchId = x.Id,
                Batch = x,
                AssessmentId = assessment.Id,
                Assessment = assessment
            });

            assessment.BatchAssessment = batchAssessment.ToList();

            foreach (var batch in batches)
            {

                await batchService.AssignAssessmentToBatchAsync(batch.Id, assessment);
            }
            _backgroundService.Schedule<IMissedSubmissionScoringService>(
                service => service.ScoreZeroForUnsubmittedAsync(assessment.Id),
                assessment.EndDate
            );

            await _unitOfWork.SaveChangesAsync();

            _backgroundService.Schedule<IAssessmentService>(service => service.UpdateAssessmentStatusAsync(assessment.Id, AssessmentStatus.InProgress), assessment.StartDate);
            _backgroundService.Schedule<IAssessmentService>(service => service.UpdateAssessmentStatusAsync(assessment.Id, AssessmentStatus.Completed), assessment.EndDate);

            return new BaseResponse<AssessmentDto>()
            {
                Status = true,
                Message = "Assessment created successfully",
                Data = new AssessmentDto()
                {
                    Id = assessment.Id,
                    Title = assessment.Title,
                    Description = assessment.Description,
                    TechnologyStack = assessment.TechnologyStack.ToString(),
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
                    TechnologyStack = x.TechnologyStack.ToString(),
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
     PaginationRequest request, AssessmentStatus? status)
        {
            var userId = _currentUser.GetCurrentUserId();
            if (userId == Guid.Empty)
                throw new ApiException("User ID is required", (int)HttpStatusCode.BadRequest, "USER_ID_REQUIRED", null);

            DateTime now = DateTime.UtcNow;

            // Build the complete filter with status included directly
            Expression<Func<Assessment, bool>> filter = x =>
             x.AssessmentAssignments.Any(a => a.StudentId == userId) &&
             (
             status == null ||
                 (status == AssessmentStatus.Upcoming) ||
                 (status == AssessmentStatus.InProgress) ||
                 (status == AssessmentStatus.Completed)
             ) &&
             x.Questions.Count() > 0;
            var assessments = await _assessmentRepository.GetAllAsync(filter, request);

            var paginationDto = new PaginationDto<AssessmentDto>
            {
                Items = assessments.Items.Select(x => new AssessmentDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    TechnologyStack = x.TechnologyStack.ToString(),
                    DurationInMinutes = x.DurationInMinutes,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    PassingScore = x.PassingScore,
                    Submitted = x.Submissions.Any(x => x.StudentId == userId)
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
                    TechnologyStack = x.TechnologyStack.ToString(),
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
                TechnologyStack = assessment.TechnologyStack.ToString(),
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
                    TechnologyStack = assessment.TechnologyStack.ToString(),
                    DurationInMinutes = assessment.DurationInMinutes,
                    StartDate = assessment.StartDate,
                    EndDate = assessment.EndDate,
                    PassingScore = assessment.PassingScore
                }
            };
        }

        public async Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAssessmentsByBatchId(Guid id, PaginationRequest request)
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
                TechnologyStack = x.TechnologyStack.ToString(),
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

        public async Task<BaseResponse<List<AssessmentPerformanceDto>>> GetTopAssessments(int? month, int? year)
        {
            var assessments = await _assessmentRepository.GetTopPerformingAssessmentsAsync(3,month,year);
            if (assessments == null || !assessments.Any())
            {
                throw new ApiException("No assessments found", (int)HttpStatusCode.NotFound, "ASSESSMENTS_NOT_FOUND", null);
            }
            return new BaseResponse<List<AssessmentPerformanceDto>>
            {
                Status = true,
                Message = "Top assessments retrieved successfully",
                Data = assessments
            };
        }

        public async Task<BaseResponse<List<AssessmentPerformanceDto>>> GetLowestAssessments(int? month, int? year)
        {
            var assessments = await _assessmentRepository.GetLowPerformingAssessmentsAsync(3, month, year);
            if (assessments == null || !assessments.Any())
            {
                throw new ApiException("No assessments found", (int)HttpStatusCode.NotFound, "ASSESSMENTS_NOT_FOUND", null);
            }
            

            return new BaseResponse<List<AssessmentPerformanceDto>>
            {
                Status = true,
                Message = "Lowest assessments retrieved successfully",
                Data = assessments
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
                TechnologyStack = x.TechnologyStack.ToString(),
                DurationInMinutes = x.DurationInMinutes,
                InstructorName = x.Instructor?.FullName ?? "Unknown Instructor",
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                CreatedAt = x.CreatedAt,
                PassingScore = x.PassingScore,
                Status = x.Status.ToString()
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
            if (userId == Guid.Empty)
            {
                throw new ApiException("Current user ID is invalid", (int)HttpStatusCode.BadRequest, "INVALID_USER_ID", null);
            }
            var recentAssessments = await _assessmentRepository.GetAllAsync(
                x => x.CreatedAt >= DateTime.UtcNow.AddDays(-7) && x.InstructorId == userId,
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
                TechnologyStack = x.TechnologyStack.ToString(),
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
        public async Task<BaseResponse<List<AssessmentPerformanceDto>>> GetInstructorAssessmentScoresAsync(DateTime? fromDate, DateTime? toDate)
        {
            var currentUserId = _currentUser.GetCurrentUserId();
            if (currentUserId == Guid.Empty)
                throw new ApiException("Current user ID is invalid", (int)HttpStatusCode.BadRequest, "INVALID_USER_ID", null);

            var check = await _userRepository.CheckAsync(x => x.Id == currentUserId);
            if (!check)
                throw new ApiException("User not found", (int)HttpStatusCode.NotFound, "USER_NOT_FOUND", null);

            var assessments = await _assessmentRepository.GetAllAsync(x =>
                x.InstructorId == currentUserId &&
                (!fromDate.HasValue || x.CreatedAt.Date >= fromDate.Value.Date) &&
                (!toDate.HasValue || x.CreatedAt.Date <= toDate.Value.Date)
            );

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

        public async Task<BaseResponse<PaginationDto<InstructorAssessmentDto>>> GetAssessmentsByInstructorAsync(Guid? batchId, AssessmentStatus? status, PaginationRequest request)
        {
            var userId = _currentUser.GetCurrentUserId();
            var now = DateTime.UtcNow;
            var assessments = await _assessmentRepository.GetAllAsync(x =>
                x.InstructorId == userId &&
                (!batchId.HasValue || x.BatchAssessment.Any(b => b.BatchId == batchId)) &&
                (status == null || x.Status == status),
                request);

            var items = assessments.Items.Select(a => new InstructorAssessmentDto
            {
                Id = a.Id,
                Title = a.Title,
                TechnologyStack = a.TechnologyStack.ToString(),
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

        public async Task<BaseResponse<AssessmentMetrics>> GetAssessmentMetrics(Guid id)
        {
            var assessment = await _assessmentRepository.GetAsync(x => x.Id == id);
            if (assessment is null)
                throw new ApiException("Assessment Not Found", (int)HttpStatusCode.BadRequest, "ASSESSMENT_NOT_FOUND", null);
            var totalSubmission = assessment.Submissions.Count;
            var assignedStudent = assessment.AssessmentAssignments.Count;
            var assessmentMetrics = new AssessmentMetrics()
            {
                AvgScore = totalSubmission > 0 ? Math.Round(assessment.Submissions.Average(x => x.TotalScore)) : 0,
                PassRate = totalSubmission > 0 ? Math.Round((assessment.Submissions.Count(s => s.TotalScore >= assessment.PassingScore) * 100.0 / totalSubmission))
                : 0,

                TotalSubmissions = totalSubmission,
                CompletionRate = assignedStudent > 0 ? Math.Round((totalSubmission * 100.0 / assignedStudent))
                : 0
            };
            return new BaseResponse<AssessmentMetrics>
            {
                Message = "Assessment Metrics",
                Status = true,
                Data = assessmentMetrics
            };
        }

        public async Task<BaseResponse<List<BatchPerformance>>> GetBatchPerformance(Guid assessmentId)
        {
            var assessment = await _assessmentRepository
                .GetForBatchPerformanceAsync(assessmentId);

            if (assessment == null)
                throw new ApiException("Assessment Not Found", 400, "ASSESSMENT_NOT_FOUND", null);

            // Group submissions by the student's batch
            var batchPerformances = assessment.Submissions
                .Where(s => s.Student != null && s.Student.BatchId != Guid.Empty)
                .GroupBy(s => new { s.Student.BatchId, s.Student.Batch.Name })
                .Select(g => new BatchPerformance
                {
                    BatchName = g.Key.Name,
                    AverageScore = Math.Round(g.Average(s => s.TotalScore), 2)
                })
                .ToList();

            return new BaseResponse<List<BatchPerformance>>()
            {
                Message = "Success",
                Status = true,
                Data = batchPerformances
            };
        }
        public async Task<BaseResponse<PaginationDto<StudentAssessmeentPerformance>>> GetStudentAssessmentPerformance(Guid assessmentId, PaginationRequest request)
        {
            var assessment = await _assessmentRepository.GetForBatchPerformanceAsync(assessmentId);
            if (assessment == null)
                throw new ApiException("Assessment Not Found", 400, "ASSESSMENT_NOT_FOUND", null);

            var studentIds = assessment.AssessmentAssignments.Select(x => x.StudentId).ToList();

            var students = await _userRepository.GetSelectedIds(studentIds, request);

            var submissionsDict = assessment.Submissions
                .ToDictionary(x => x.StudentId, x => x);

            var studentPerformance = students.Items.Select(student =>
            {
                var hasSubmission = submissionsDict.TryGetValue(student.Id, out var submission);

                return new StudentAssessmeentPerformance
                {
                    StudentId = student.Id,
                    Name = student.FullName,
                    Batch = student.Batch != null ? $"{student.Batch.Name} {student.Batch.BatchNumber}" : "N/A",
                    Score = hasSubmission ? submission.TotalScore : 0,
                    SubmittedAt = hasSubmission ? submission.SubmittedAt : (DateTime?)null,
                    Status = hasSubmission
                        ? (submission.TotalScore >= assessment.PassingScore ? "Passed" : "Failed")
                        : "Not Submitted"
                };
            }).ToList();
            var paginationDto = new PaginationDto<StudentAssessmeentPerformance>()
            {
                Items = studentPerformance,
                HasNextPage = students.HasNextPage,
                HasPreviousPage = students.HasPreviousPage,
                CurrentPage = students.CurrentPage,
                TotalItems = students.TotalItems,
                PageSize = students.PageSize,
                TotalPages = students.TotalPages
            };

            return new BaseResponse<PaginationDto<StudentAssessmeentPerformance>>
            {
                Message = "Success",
                Status = true,
                Data = paginationDto
            };
        }

        public async Task<BaseResponse<List<AssessmentScoreDistribution>>> GetAssessmentScoreDistribution(Guid assessmentId)
        {
            var assessment = await _assessmentRepository.GetForBatchPerformanceAsync(assessmentId);
            if (assessment == null)
                throw new ApiException("Assessment Not Found", 400, "ASSESSMENT_NOT_FOUND", null);

            var scores = assessment.Submissions.Select(x => x.TotalScore).ToList();

            var caps = new List<AssessmentScoreDistribution>();

            for (int lower = 0; lower <= 100; lower += 10)
            {
                int upper = lower + 9;
                if (upper > 100) upper = 100;

                caps.Add(new AssessmentScoreDistribution
                {
                    Cap = $"{lower}-{upper}",
                    Count = scores.Count(x => x >= lower && x <= upper)
                });
            }

            return new BaseResponse<List<AssessmentScoreDistribution>>()
            {
                Status = true,
                Message = "Assessment Score Distribution",
                Data = caps
            };
        }
        public async Task UpdateAssessmentStatusAsync(Guid assessmentId, AssessmentStatus newStatus)
        {
            var assessment = await _assessmentRepository.GetAsync(assessmentId);
            if (assessment == null) throw new Exception("Assessment not found");

            assessment.Status = newStatus;
            _assessmentRepository.Update(assessment);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<BaseResponse<PaginationDto<AdminAssessmentDto>>> GetAllAssessmentsAsync(
    Guid? batchId, DateTime? startDate, DateTime? endDate, string search, PaginationRequest request)
        {
            var query = await _assessmentRepository.GetAllAsync(x => (!batchId.HasValue || x.BatchAssessment.Any(a => a.BatchId == batchId.Value)) && (!startDate.HasValue || x.StartDate.Date >= startDate.Value.Date) && (!endDate.HasValue || x.EndDate.Date <= endDate.Value.Date) &&
                (
                    string.IsNullOrEmpty(search) ||
                    x.Title.Contains(search) || x.Instructor.FullName.Contains(search)
                ), request);

            var assessments = query.Items.Select(x => new AdminAssessmentDto
            {
                Id = x.Id,
                Title = x.Title,
                TechnologyStack = x.TechnologyStack.ToString(),
                DurationInMinutes = x.DurationInMinutes,
                StartDate = x.StartDate,
                TotalScore = x.Questions.Sum(b => b.Marks),
                EndDate = x.EndDate,
                PassingScore = x.PassingScore,
                Status = x.Status.ToString(),
                InstructorName = x.Instructor.FullName,
                Batches = x.BatchAssessment.Select(b => new BatchDto
                {
                    Id = b.Id,
                    Name = b.Batch.Name
                }).ToList()
            }).ToList();


            var response = new PaginationDto<AdminAssessmentDto>
            {
                Items = assessments,
                TotalItems = query.TotalItems,
                TotalPages = query.TotalPages,
                CurrentPage = query.CurrentPage,
                PageSize = query.PageSize,
                HasNextPage = query.HasNextPage,
                HasPreviousPage = query.HasPreviousPage

            };

            return new BaseResponse<PaginationDto<AdminAssessmentDto>>
            {
                Data = response,
                Status = true,
                Message = "Assessments fetched successfully"
            };
        }
        public async Task<AssessmentOverviewDto> GetAssessmentOverviewAsync(Guid assessmentId)
        {
            if (assessmentId == Guid.Empty)
                throw new ApiException("Assessment ID is required", 400, "ASSESSMENT_ID_REQUIRED", null);
            var assessment = await _assessmentRepository.GetForOverview(assessmentId);

            if (assessment == null)
                throw new ApiException("Assessment Not Found", 400, "ASSESSMENT_NOT_FOUND", null);

            // Get assigned batch names (null-safe)
            var batchesAssigned = assessment.BatchAssessment?
                .Select(x => x?.Batch?.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .ToList() ?? new List<string>();

            var assignments = assessment.AssessmentAssignments ?? new List<AssessmentAssignment>();
            var submissions = assessment.Submissions ?? new List<Submission>();

            var totalAssigned = assignments.Count;
            var submitted = submissions.Count(x => x.IsAutoSubmitted == false);

            // Not submitted = assigned students that do not have a submission
            var submittedStudentIds = submissions.Select(s => s.StudentId).ToHashSet(); 
            var autoSubmittedStudentIds = submissions
                .Where(s => s.IsAutoSubmitted)
                .Select(s => s.StudentId)
                .ToHashSet();

            var notSubmitted = assignments.Count(a =>
                !submittedStudentIds.Contains(a.StudentId) ||
                !autoSubmittedStudentIds.Contains(a.StudentId));


            var passed = submissions.Count(s => s.TotalScore >= assessment.PassingScore);
            var failed = submissions.Count(s => s.TotalScore < assessment.PassingScore);

            return new AssessmentOverviewDto
            {
                AssignedBatches = batchesAssigned,
                TotalAssignedStudents = totalAssigned,
                SubmittedCount = submitted,
                NotSubmittedCount = notSubmitted,
                PassedCount = passed,
                FailedCount = failed
            };
        }
            
        public async Task<BaseResponse<PaginationDto<GroupedStudentDto>>> GetGroupedStudentsAsync(Guid assessmentId, AssessmentStudentGroupType type, PaginationRequest request)
        {
            var assessment = await _assessmentRepository.GetAsync(x => x.Id == assessmentId);
            if (assessment == null)
                throw new ApiException("Assessment not found", 404, "ASSESSMENT_NOT_FOUND");

            var passingScore = assessment.PassingScore;

            Expression<Func<User, bool>> filter = type switch
            {
                AssessmentStudentGroupType.Submitted => user =>
                    user.Submissions.Any(sub => sub.AssessmentId == assessmentId && sub.IsAutoSubmitted == false),

                AssessmentStudentGroupType.NotSubmitted => user =>
                    user.AssessmentAssignments.Any(aa => aa.AssessmentId == assessmentId) &&
                    user.Submissions.Any(sub => sub.AssessmentId == assessmentId && sub.IsAutoSubmitted == true),

                AssessmentStudentGroupType.Passed => user =>
                    user.Submissions.Any(sub => sub.AssessmentId == assessmentId && sub.TotalScore >= passingScore),

                AssessmentStudentGroupType.Failed => user =>
                    user.Submissions.Any(sub => sub.AssessmentId == assessmentId && sub.TotalScore < passingScore),

                _ => throw new ApiException("Invalid group type", 400, "INVALID_GROUP_TYPE")
            };

            var students = await _userRepository.GetAllForGroupedStudentAsync(filter,request);

            var studentsList = students.Items.Select(s =>
            {
                var submission = s.Submissions.FirstOrDefault(sub => sub.AssessmentId == assessmentId);

                return new GroupedStudentDto
                {
                    StudentId = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    BatchName = s.Batch?.Name,
                    TotalScore = submission?.TotalScore
                };
            }).ToList();
            var Paginated = new PaginationDto<GroupedStudentDto>()
            {
                TotalItems = students.TotalItems,
                TotalPages = students.TotalPages,
                HasNextPage = students.HasNextPage,
                HasPreviousPage = students.HasPreviousPage,
                Items = studentsList,
                CurrentPage = request.CurrentPage,
                PageSize = request.PageSize
            };
            return new BaseResponse<PaginationDto<GroupedStudentDto>>
            {
                Data = Paginated,
                Message = "Grouped student Retrieved",
                Status = true
            };
        }


    }

}
