using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entitties;

namespace Application.Services
{
    public class MissedSubmissionScoringService : IMissedSubmissionScoringService
    {
        private readonly IAssessmentRepository _assessmentRepo;
        private readonly ISubmissionRepository _submissionRepo;
        private readonly IUnitOfWork _unitOfWork;

        public MissedSubmissionScoringService(
            IAssessmentRepository assessmentRepo,
            ISubmissionRepository submissionRepo,
            IUnitOfWork unitOfWork)
        {
            _assessmentRepo = assessmentRepo;
            _submissionRepo = submissionRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task ScoreZeroForUnsubmittedAsync(Guid assessmentId)
        {
            var assessment = await _assessmentRepo.GetAsync(assessmentId);

            if (assessment == null || assessment.EndDate > DateTime.UtcNow)
                return; // Either assessment doesn't exist or hasn't ended yet

            // 1. Get assigned students
            var assignedStudents = assessment.AssessmentAssignments
                .Select(aa => aa.Student)
                .ToList();

            // 2. Get students who already submitted
            var submittedStudentIds = assessment.Submissions
                .Select(s => s.StudentId)
                .ToHashSet();

            // 3. Find students who didn’t submit
            var missingStudents = assignedStudents
                .Where(s => !submittedStudentIds.Contains(s.Id))
                .ToList();

            // 4. Create zero-score submissions for missing students
            foreach (var student in missingStudents)
            {
                var missedSubmission = new Submission()
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    AssessmentId = assessment.Id,
                    SubmittedAt = assessment.EndDate,
                    TotalScore = 0,
                    FeedBack = "Did not submit - auto scored 0",
                    IsAutoSubmitted = true,
                    AnswerSubmissions = new List<AnswerSubmission>()
                };

                await _submissionRepo.CreateAsync(missedSubmission);
            }

            await _unitOfWork.SaveChangesAsync();
        }



    }

}
