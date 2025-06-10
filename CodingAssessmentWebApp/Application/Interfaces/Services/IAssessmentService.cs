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
        Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAllAssessmentsByInstructorIdAsync( PaginationRequest request, Guid instructorId = default);
        Task<BaseResponse<PaginationDto<AssessmentDto>>> GetAllStudentAssessments(PaginationRequest request, Guid studentId = default);

        Task<BaseResponse<AssessmentDto>> AssignStudents(Guid id, AssignStudentsModel model);
        Task<BaseResponse<AssessmentDto>> UpdateAssessmentAsync(Guid id, UpdateAssessmentRequestModel model);
    }
}
