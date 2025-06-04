using System.Net;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public class DashboardService(ICurrentUser _currentUser, IAssessmentRepository _assessmentRepository, ISubmissionRepository _submissionRepository) : IDashboardService
    {
        public async Task<BaseResponse<StudentDashBoardDto>> GetInstructorDashboardAsync()
        {
            var instructorId = _currentUser.GetCurrentUserId();
            if (instructorId == Guid.Empty)
                throw new ApiException("Current user ID is not set or invalid.", (int)HttpStatusCode.Forbidden, "InvalidUserId", null);

            var assessments = await _assessmentRepository.GetAllAsync(a => a.InstructorId == instructorId);
            if (assessments?.Any() != true)
                throw new ApiException("No assessments found for the instructor.", (int)HttpStatusCode.NotFound, "NoAssessmentsFound", null);

            var assessmentIds = assessments.Select(a => a.Id).ToList();
            var submissions = await _submissionRepository.GetSelectedIds(assessmentIds);
            if (submissions?.Any() != true)
                throw new ApiException("No submissions found for the assessments.", (int)HttpStatusCode.NotFound, "NoSubmissionsFound", null);

            int totalAssessments = assessments.Count();
            int activeAssessments = assessments.Count(a => a.EndDate >= DateTime.UtcNow);
            int totalStudents = submissions.Select(s => s.StudentId).Distinct().Count();

            double averageScore = submissions.Average(s => s.TotalScore);
            double completionRate = (double)submissions.Select(s => s.AssessmentId).Distinct().Count() / totalAssessments * 100;

            var topStudents = submissions
                .GroupBy(s => s.StudentId)
                .Select(g => new TopStudentDto
                {
                    StudentId = g.Key,
                    StudentName = g.First().Student.FullName,
                    AverageScore = g.Average(s => s.TotalScore)
                })
                .OrderByDescending(t => t.AverageScore)
                .Take(5)
                .ToList();

            var dashboardDto = new StudentDashBoardDto
            {
                TotalAssessments = totalAssessments,
                ActiveAssessments = activeAssessments,
                TotalStudentsInvited = totalStudents,
                AverageScore = averageScore,
                CompletionRate = completionRate,
                TopStudents = topStudents,
            };

            return new BaseResponse<StudentDashBoardDto>
            {
                Message = "Dashboard data retrieved successfully.",
                Status = true,
                Data = dashboardDto
            };
        }

        public async Task<BaseResponse<StudentDashBoardDto>> GetStudentDashboardAsync()
        {
            var currentUserId = _currentUser.GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                throw new ApiException("Current user ID is not set or invalid.", (int)HttpStatusCode.Forbidden, "InvalidUserId", null);
            }

            var submissions = await _submissionRepository.GetAllAsync(x => x.StudentId == currentUserId);
            if (submissions == null || !submissions.Any())
            {
                throw new ApiException("No submissions found for the current user.", (int)HttpStatusCode.NotFound, "NoSubmissionsFound", null);
            }

            var averageScore = submissions.Average(s => s.TotalScore);
            var assessment = submissions.Select(x => x.Assessment);
            var assessmentCount = assessment.Count();
            var completedAssessmentsCount = submissions.Count(x => x.AnswerSubmissions.Count == x.Assessment.Questions.Count);
            var completionRate = (double)completedAssessmentsCount / assessmentCount * 100;

            var recentAssessments = assessment.Select(x => new AssessmentDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                TechnologyStack = x.TechnologyStack,
                DurationInMinutes = x.DurationInMinutes,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                PassingScore = x.PassingScore
            }).OrderByDescending(x => x.EndDate).Take(5).ToList();

            return new BaseResponse<StudentDashBoardDto>
            {
                Message = "Student dashboard data retrieved successfully.",
                Status = true,
                Data = new StudentDashBoardDto
                {
                    //AverageScore = averageScore,
                    //CompletedAssessmentsCount = completedAssessmentsCount,
                    //TotalAssessmentsCount = assessmentCount,
                    //CompletionRate = completionRate,
                    //RecentAssessments = recentAssessments
                }
            };
        }
    }
    
}
