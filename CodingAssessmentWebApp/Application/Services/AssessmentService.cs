using System.Net;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.ExternalServices;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entitties;
using Domain.Enum;

namespace Application.Services
{
    public class AssessmentService(IUnitOfWork _unitOfWork, IAssessmentRepository _assessmentRepository, IUserRepository _userRepository, IBackgroundService _backgroundService, ICurrentUser _currentUser) : IAssessmentService
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

            var assessment = new Assessment()
            {
                Title = model.Title,
                Description = model.Description,
                TechnologyStack = model.TechnologyStack,
                DurationInMinutes = model.DurationInMinutes,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                PassingScore = model.PassingScore,
                InstructorId = user.Id
            };
            await _assessmentRepository.CreateAsync(assessment);
            if (model.AssignedStudentIds is not null && model.AssignedStudentIds.Count > 0)
            {
                var validStudentIds = await _userRepository.GetAllAsync(x => model.AssignedStudentIds.Contains(x.Id));

                if (validStudentIds.Count != model.AssignedStudentIds.Count)
                {
                    throw new ApiException("Some student IDs are invalid or not students.", (int)HttpStatusCode.BadRequest, "INVALID_STUDENT_IDS", null);
                }
                if (validStudentIds != null && validStudentIds.Any())
                {
                    assessment.AssessmentAssignments = validStudentIds.Select(x => new AssessmentAssignment
                    {
                        StudentId = x.Id,
                        Student = x,
                        AssessmentId = assessment.Id,
                        Assessment = assessment
                    }).ToList();
                }
                _backgroundService.Enqueue<IEmailService>(emailService => emailService.SendBulkEmailAsync(validStudentIds, "New Assessment", new AssessmentDto()
                {
                    Title = assessment.Title,
                    Description = assessment.Description,
                    TechnologyStack = assessment.TechnologyStack,
                    DurationInMinutes = assessment.DurationInMinutes,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    PassingScore = model.PassingScore
                }));
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
        public async Task<BaseResponse<PaginationDto<AssessmentDto>>> GetCurrentStudentAssessments(PaginationRequest request)
        {
            var userId = _currentUser.GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                throw new ApiException("User ID is required", (int)HttpStatusCode.BadRequest, "USER_ID_REQUIRED", null);
            }
            return await GetAllStudentAssessments(request, userId);
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

        public Task<BaseResponse<AssessmentDto>> GetAssessmentAsync(Guid id)
        {
            throw new NotImplementedException();
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
    }
}
