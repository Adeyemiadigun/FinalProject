using System.Globalization;
using System.Linq.Expressions;
using System.Net;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Entitties;
using Domain.Enum;

namespace Application.Services
{
    public class DashboardService(ICurrentUser _currentUser, IAssessmentRepository _assessmentRepository, ISubmissionRepository _submissionRepository, IBatchRepository batchRepository) : IDashboardService
    {
        public async Task<BaseResponse<AdminDashBoardOverview>> AdminDashBoardOverview()
        {

            var assessments = await _assessmentRepository.GetAllAsync(a => a != null);
            if (assessments?.Any() != true)
                throw new ApiException("No assessments found for the instructor.", (int)HttpStatusCode.NotFound, "NoAssessmentsFound", null);

            var assessmentIds = assessments.Select(a => a.Id).ToList();
            var submissions = await _submissionRepository.GetSelectedIds(assessmentIds);
            if (submissions?.Any() != true)
                throw new ApiException("No submissions found for the assessments.", (int)HttpStatusCode.NotFound, "NoSubmissionsFound", null);

            int totalAssessments = assessments.Count();
            int activeAssessments = assessments.Count(a => a.EndDate >= DateTime.UtcNow && a.StartDate <= DateTime.UtcNow);
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
            var lowestStudents = submissions
                .GroupBy(s => s.StudentId)
                .Select(g => new LowestPerformingStudent
                {
                    StudentId = g.Key,
                    StudentName = g.First().Student.FullName,
                    AverageScore = g.Average(s => s.TotalScore)
                })
                .OrderBy(t => t.AverageScore)
                .Take(5)
                .ToList();
            var batches = await batchRepository.GetAllBatchesAsync();
            if (batches == null || !batches.Any())
                throw new ApiException("No batches found.", (int)HttpStatusCode.NotFound, "NoBatchesFound", null);
            var dashboardDto = new AdminDashBoardOverview
            {
                TotalAssessments = totalAssessments,
                ActiveAssessments = activeAssessments,
                TotalStudents = totalStudents,
                AverageScore = averageScore,
                CompletionRate = completionRate,
                TopStudents = topStudents,
                TotalBatches = batches.Count(),
                LowestStudents = lowestStudents
            };
//            GET / api / v1 / admin / metrics / overview
//Response:
//            {
//                "totalStudents": 248,
//  "totalAssessments": 34,
//  "totalBatches": 12,
//  "averagePassRate": 72

            return new BaseResponse<AdminDashBoardOverview>
            {
                Message = "Dashboard data retrieved successfully.",
                Status = true,
                Data = dashboardDto
            };
        }

        public async Task<BaseResponse<AssessmentMetricsDto>> GetAssessmentMetrics(Guid? instructorId, Guid? batchId)
        {
            ICollection<Assessment> assessmentsQuery = new List<Assessment>(); // Fixed initialization

            if (instructorId != Guid.Empty && batchId == Guid.Empty)
            {
                assessmentsQuery = await _assessmentRepository.GetAllAsync(x => x.InstructorId == instructorId);
            }

            if (batchId != Guid.Empty && instructorId == Guid.Empty)
            {
                assessmentsQuery = await _assessmentRepository.GetAllAsync(x => x.BatchAssessment.Any(ba => ba.BatchId == batchId));
            }
            if (instructorId != Guid.Empty && batchId != Guid.Empty)
            {
                assessmentsQuery = await _assessmentRepository.GetAllAsync(x => x.InstructorId == instructorId && x.BatchAssessment.Any(ba => ba.BatchId == batchId));
            }
            if (instructorId == Guid.Empty && batchId == Guid.Empty)
            {
                assessmentsQuery = await _assessmentRepository.GetAllAsync(x => x != null);
            }
            if (!assessmentsQuery.Any())
            {
               throw new ApiException("No assessments found for the specified criteria.", (int)HttpStatusCode.NotFound, "NoAssessmentsFound", null);
            }

            var totalAssessments = assessmentsQuery.Count;
            var activeAssessments = assessmentsQuery.Count(a => a.EndDate > DateTime.UtcNow);
            var allSubmissions = assessmentsQuery.SelectMany(a => a.Submissions).ToList();
            var totalSubmissions = allSubmissions.Count;

            double averageScore = totalSubmissions > 0
                ? allSubmissions.Average(s => s.TotalScore)
                : 0;

            double passRate = totalSubmissions > 0
                ? (allSubmissions.Count(s => s.TotalScore >= s.Assessment.PassingScore) * 100.0 / totalSubmissions)
                : 0;

            var assignedStudentCount = assessmentsQuery
     .SelectMany(x => x.AssessmentAssignments)
     .Select(aa => aa.StudentId)
     .Distinct()
     .Count();

            double completionRate = assignedStudentCount > 0
                ? (totalSubmissions * 100.0 / assignedStudentCount)
                : 0;

            var dto = new AssessmentMetricsDto()
            {
                TotalAssessments = totalAssessments,
                ActiveAssessments = activeAssessments,
                AverageScore = Math.Round(averageScore, 2),
                PassRate = Math.Round(passRate, 2),
                CompletionRate = Math.Round(completionRate, 2)
            };

            return new BaseResponse<AssessmentMetricsDto> 
            {
                Message = "Assessment metrics retrieved successfully.",
                Status = true,
                Data = dto
            };
        }

        

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

        public async Task<BaseResponse<List<QuestionTypeMetrics>>> GetQuestionTypeMetrics(Guid? instructorId, Guid? batchId)
        {
            // Step 1: Fetch and filter assessments based on parameters
            var assessments = await _assessmentRepository.GetAllAsync(x =>
                (instructorId == null || x.InstructorId == instructorId) &&
                (batchId == null || x.BatchAssessment.Any(ba => ba.BatchId == batchId))
            );

            // Step 2: Aggregate question scores by type using LINQ
            var questionScores = assessments
                .SelectMany(a => a.Submissions.SelectMany(s => s.AnswerSubmissions
                    .Select(ans => new
                    {
                        QuestionType = a.Questions.FirstOrDefault(q => q.Id == ans.QuestionId)?.QuestionType,
                        Score = ans.Score
                    })))
                .Where(x => x.QuestionType != null)
                .GroupBy(x => x.QuestionType)
                .Select(g => new QuestionTypeMetrics
                {
                    QuestionType = g.Key.ToString(),
                    AverageScore = Math.Round(g.Average(x => x.Score), 2)
                })
                .ToList();

            // Step 3: Handle empty result
            if (!questionScores.Any())
            {
                throw new ApiException(
                    "No question type metrics found.",
                    (int)HttpStatusCode.NotFound,
                    "NoQuestionTypeMetricsFound",
                    null
                );
            }

            // Step 4: Return successful response
            return new BaseResponse<List<QuestionTypeMetrics>>
            {
                Data = questionScores,
                Status = true,
                Message = "Question type metrics retrieved successfully."
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
        public async Task<BaseResponse<List<BatchAnalytics>>> BatchAnalytics(Guid? instructorId, Guid? batchId)
        {
            var batches = await batchRepository.GetAllAsync(x =>
                batchId == null || x.Id == batchId
            );

            if (batches is null || !batches.Any())
            {
                throw new ApiException(
                    "No batches found.",
                    404,
                    "NoBatchesFound",
                    null
                );
            }

            var batchAnalytics = batches.Select(batch =>
            {
                var students = batch.Students;

                var filteredStudentSubmissions = students.Select(s => new
                {
                    Student = s,
                    Submissions = s.Submissions
                        .Where(sub => !instructorId.HasValue || sub.Assessment.InstructorId == instructorId)
                        .ToList()
                }).ToList();

                double averageScore = filteredStudentSubmissions
                    .Select(ss => ss.Submissions.Sum(sub => sub.TotalScore))
                    .DefaultIfEmpty(0)
                    .Average();

                double passRate = students.Any()
                    ? filteredStudentSubmissions
                        .Count(ss => ss.Submissions.Any(sub => sub.TotalScore >= sub.Assessment.PassingScore))
                        / (double)students.Count * 100
                    : 0;

                return new BatchAnalytics
                {
                    Id = batch.Id,
                    Name = batch.Name + batch.BatchNumber,
                    StudentCount = students.Count,
                    AverageScore = Math.Round(averageScore, 2),
                    PassRate = Math.Round(passRate, 2)
                };
            }).ToList();
            return new BaseResponse<List<BatchAnalytics>>
            {
                Data = batchAnalytics,
                Status = true,
                Message = "Batch analytics retrieved successfully."
            };
        }
        public async Task<BaseResponse<List<ScoreTrenddto>>> GetScoreTrendsAsync(Guid? instructorId, Guid? batchId)
        {
            var assessments = await _assessmentRepository.GetAllAsync(x => x != null);
            if (assessments == null || !assessments.Any())
                throw new ApiException("No assessments found.", (int)HttpStatusCode.NotFound, "NoAssessmentsFound", null);

            if (instructorId.HasValue)
                assessments = assessments.Where(a => a.InstructorId == instructorId.Value).ToList();

            if (batchId.HasValue)
                assessments = assessments.Where(a => a.BatchAssessment.Any(ba => ba.BatchId == batchId.Value)).ToList();

            var trends = assessments
                .SelectMany(a => a.Submissions)
                .GroupBy(s => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(s.SubmittedAt, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                .Select(g => new ScoreTrenddto
                {
                    Label = $"Week {g.Key}",
                    AverageScore = Math.Round(g.Average(x => x.TotalScore), 2)
                })
                .OrderBy(x => x.Label)
                .ToList();

            return new BaseResponse<List<ScoreTrenddto>>
            {
                Message = "Score trend data generated.",
                Status = true,
                Data = trends
            };
        }

        public async Task<BaseResponse<List<AssessmentCreatedDto>>> GetAssessmentsCreatedTrendAsync(Guid? instructorId, Guid? batchId)
        {
            var assessments = await _assessmentRepository.GetAllAsync(x => x != null);
            if (assessments == null || !assessments.Any())
                throw new ApiException("No assessments found.", (int)HttpStatusCode.NotFound, "NoAssessmentsFound", null);

            if (instructorId.HasValue)
                assessments = assessments.Where(a => a.InstructorId == instructorId.Value).ToList();

            if (batchId.HasValue)
                assessments = assessments.Where(a => a.BatchAssessment.Any(ba => ba.BatchId == batchId.Value)).ToList();

            var trends = assessments
                .GroupBy(a => a.CreatedAt.ToString("MMM"))
                .Select(g => new AssessmentCreatedDto
                {
                    Label = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => DateTime.ParseExact(x.Label, "MMM", CultureInfo.InvariantCulture))
                .ToList();

            return new BaseResponse<List<AssessmentCreatedDto>>
            {
                Message = "Assessment creation trend generated.",
                Status = true,
                Data = trends
            };
        }

    }

}
