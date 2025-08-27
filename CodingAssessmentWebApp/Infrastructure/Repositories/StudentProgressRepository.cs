using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;

namespace Infrastructure.Repositories
{
    public class StudentProgressRepository : BaseRepository<StudentAssessmentProgress>, IStudentProgressRepository
    {
        public StudentProgressRepository(ClhAssessmentAppDpContext context) : base(context)
        {
        }

        public async Task DeleteAssessmentProgress(Guid progressId, Guid studentId)
        {
             _context.StudentAssessmentProgresses
                .Where(p => p.Id == progressId && p.StudentId == studentId)
                .ExecuteDelete();
        }

        public async Task<StudentAssessmentProgress?> GetByStudentAndAssessmentAsync(Guid studentId, Guid assessmentId)
        {
            return await _context.StudentAssessmentProgresses
                .Include(p => p.Answers)
                    .ThenInclude(a => a.SelectedOptions)
                .FirstOrDefaultAsync(p => p.StudentId == studentId && p.AssessmentId == assessmentId);
        }
        public async Task RemoveAnswersAndOptionsAsync(StudentAssessmentProgress progress)
        {
            _context.InProgressSelectedOptions.RemoveRange(progress.Answers.SelectMany(a => a.SelectedOptions));
            _context.InProgressAnswers.RemoveRange(progress.Answers);
            await Task.CompletedTask;
        }

    }
}
