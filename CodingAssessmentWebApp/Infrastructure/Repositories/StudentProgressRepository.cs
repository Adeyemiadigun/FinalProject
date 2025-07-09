using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure.Repositories
{
    public class StudentProgressRepository : BaseRepository<StudentAssessmentProgress>, IStudentProgressRepository
    {
        public StudentProgressRepository(ClhAssessmentAppDpContext context) : base(context)
        {
        }

        public async Task<StudentAssessmentProgress?> GetByStudentAndAssessmentAsync(Guid studentId, Guid assessmentId)
        {
            return await _context.StudentAssessmentProgresses
                .Include(p => p.Answers)
                    .ThenInclude(a => a.SelectedOptions)
                .FirstOrDefaultAsync(p => p.StudentId == studentId && p.AssessmentId == assessmentId);
        }

    }

}
