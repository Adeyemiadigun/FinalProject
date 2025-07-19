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

        public async Task RunAsync()
        {
            var now = DateTime.UtcNow;
            var sevenDaysAgo = now.AddDays(-7);

            // 1. Get assessments that have ended in the past 7 days and not fully submitted
            var expiredAssessments = await _assessmentRepo.GetAllAsync(a =>
                a.EndDate < now &&
                a.EndDate >= sevenDaysAgo &&
                a.AssessmentAssignments.Count != a.Submissions.Count
            );

            foreach (var assessment in expiredAssessments)
            {
                // 2. Get assigned students (fully loaded)
                var assignedStudents = assessment.AssessmentAssignments
                    .Select(aa => aa.Student)
                    .ToList();

                // 3. Get students who already submitted
                var submittedStudentIds = assessment.Submissions
                    .Select(s => s.StudentId)
                    .ToHashSet();

                // 4. Find students who didn’t submit
                var missingStudents = assignedStudents
                    .Where(s => !submittedStudentIds.Contains(s.Id))
                    .ToList();

                // 5. Create zero-score submissions for missing students
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
                        AnswerSubmissions = new List<AnswerSubmission>()
                    };

                    await _submissionRepo.CreateAsync(missedSubmission);
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }


    }

}
