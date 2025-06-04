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
    public class AssessmentService(IUnitOfWork _unitOfWork, IAssessmentRepository _assessmentRepository, IUserRepository _userRepository, IBackgroundService _backgroundService) : IAssessmentService
    {
                 //case 400: // Bad Request
                 //   case 401: // Unauthorized
                 //   case 403: // Forbidden
                 //   case 404: // Not Found
                 //   case 409: // Conflict
                 //   case 422: // Unprocessable Entity
        public async Task<BaseResponse<AssessmentDto>> AssignStudents(AssignStudentsModel model)
        {
            if (model.AssessmentId == Guid.Empty || model.StudentIds == null || model.StudentIds.Count == 0)
            {
                throw new ApiException("Invalid input data", (int)HttpStatusCode.BadRequest, "INVALID_INPUT_DATA", null);
            }
            var assessment = await _assessmentRepository.GetAsync(model.AssessmentId);
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
            var assessment = new Assessment()
            {
                Title = model.Title,
                Description = model.Description,
                TechnologyStack = model.TechnologyStack,
                DurationInMinutes = model.DurationInMinutes,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                PassingScore = model.PassingScore,
            };
            if (model.AssignedStudentIds == null || model.AssignedStudentIds.Count <= 0)
            {
                throw new ApiException("AssignedStudentIds cannot be null or empty", (int)HttpStatusCode.BadRequest, "INVALID_REQUEST_DATA", null);
            }

            var validStudentIds = await _userRepository.GetAllAsync(x => model.AssignedStudentIds.Contains(x.Id));

            if (validStudentIds.Count != model.AssignedStudentIds.Count)
            {
                throw new ApiException("Some student IDs are invalid or not students.", (int)HttpStatusCode.BadRequest, "INVALID_STUDENT_IDS", null);
            }
            assessment.AssessmentAssignments = validStudentIds.Select(x => new AssessmentAssignment()
            {
                StudentId = x.Id,
                Student = x,
                AssessmentId = assessment.Id,
                Assessment = assessment,
            }).ToList();

            await _assessmentRepository.CreateAsync(assessment);
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

        public Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAllAssessmentsByCourseIdAsync(Guid courseId)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAllAssessmentsByInstructorIdAsync(Guid instructorId, PaginationRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResponse<AssessmentDto>> GetAssessmentAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
