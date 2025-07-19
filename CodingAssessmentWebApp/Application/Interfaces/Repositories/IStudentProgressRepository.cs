
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IStudentProgressRepository : IBaseRepository<StudentAssessmentProgress>
    {
        Task<StudentAssessmentProgress?> GetByStudentAndAssessmentAsync(Guid studentId, Guid assessmentId);
        Task RemoveAnswersAndOptionsAsync(StudentAssessmentProgress progress);
    }

}
