using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;

namespace Application.Interfaces.Services
{
    public interface IAssessmentService
    {
        Task<BaseResponse<AssessmentDto>> CreateAssessmentAsync(CreateAssessmentRequestModel model);
        Task<BaseResponse<AssessmentDto>> GetAssessmentAsync(Guid id);
        Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAllAssessmentsAsync(PaginationRequest request);
        Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAllAssessmentsByCourseIdAsync(Guid courseId);
        Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAllAssessmentsByInstructorIdAsync(Guid instructorId, PaginationRequest request);
        Task<BaseResponse<AssessmentDto>> AssignStudents(AssignStudentsModel model);
    }
}
