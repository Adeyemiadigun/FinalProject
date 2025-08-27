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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Services
{
    public class DashboardService(ICurrentUser _currentUser, IAssessmentRepository _assessmentRepository, ISubmissionRepository _submissionRepository, IBatchRepository batchRepository, IUserRepository _userRepository, ILeaderboardStore _leaderboardStore) : IDashboardService
    {
        public async Task<BaseResponse<AdminDashBoardOverview>> AdminDashBoardOverview()
        {

            var assessments = await _assessmentRepository.GetAllAsync(a => a != null);
            if (assessments?.Any() != true)
                throw new ApiException("No assessments found for the instructor.", (int)HttpStatusCode.NotFound, "NoAssessmentsFound", null);

            var assessmentIds = assessments.Select(a => a.Id).ToList();
            var submissions = await _submissionRepository.GetAllAsync(x => assessmentIds.Contains(x.AssessmentId));
            if (submissions?.Any() != true)
                throw new ApiException("No submissions found for the assessments.", (int)HttpStatusCode.NotFound, "NoSubmissionsFound", null);
            var students = await _userRepository.GetAllAsync(x => x.Role == Role.Student && x.IsActive == true);

            int totalAssessments = assessments.Count();
            int activeAssessments = assessments.Count(a => a.EndDate >= DateTime.UtcNow && a.StartDate <= DateTime.UtcNow);
            int totalStudents = submissions.Select(s => s.StudentId).Distinct().Count();

            double averageScore = Math.Round(submissions.Average(s => s.TotalScore), 2);
            double completionRate = Math.Round(
                (double)submissions.Select(s => s.AssessmentId).Distinct().Count() / totalAssessments * 100, 2);


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
                TotalStudents = students.Count(),
                AverageScore = averageScore,
                CompletionRate = completionRate,
                TopStudents = topStudents,
                TotalBatches = batches.Count(),
                LowestStudents = lowestStudents
            };

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

            if (instructorId is not null && batchId is null)
            {
                assessmentsQuery = await _assessmentRepository.GetAllAsync(x => x.InstructorId == instructorId);
            }

            if (batchId is not null && instructorId is null)
            {
                assessmentsQuery = await _assessmentRepository.GetAllAsync(x => x.BatchAssessment.Any(ba => ba.BatchId == batchId));
            }
            if (instructorId is not null && batchId is not null)
            {
                assessmentsQuery = await _assessmentRepository.GetAllAsync(x => x.InstructorId == instructorId && x.BatchAssessment.Any(ba => ba.BatchId == batchId));
            }
            if (instructorId is null && batchId is null)
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
                ? (allSubmissions.Count(s => s.TotalScore >= s.Assessment.RequiredPassingScore) * 100.0 / totalSubmissions)
                : 0;

            var assignedStudentCount = assessmentsQuery
                 .SelectMany(x => x.AssessmentAssignments)
                 .Select(aa => aa.StudentId)
                 .Distinct()
                 .Count();
            double totalAssigned = 0;
            double totalCompleted = 0;
            foreach (var assessment in assessmentsQuery)
            {
                var assigned = assessment.AssessmentAssignments.Select(x => x.StudentId).Distinct().Count();
                var submitted = assessment.Submissions.Select(x => x.StudentId).Distinct().Count();

                totalAssigned += assigned;
                totalCompleted += submitted;
            }

            double completionRate = totalAssigned > 0
                ? (totalCompleted * 100.0 / totalAssigned)
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

        

        public async Task<BaseResponse<InstructorDashboardOverview>> GetInstructorDashboardAsync()
        {
            var instructorId = _currentUser.GetCurrentUserId();
            if (instructorId == Guid.Empty)
                throw new ApiException("Current user ID is not set or invalid.", (int)HttpStatusCode.Forbidden, "InvalidUserId", null);

            var assessments = await _assessmentRepository.GetAllAsync(a => a.InstructorId == instructorId);
            if (assessments?.Any() != true)
                throw new ApiException("No assessments found for the instructor.", (int)HttpStatusCode.NotFound, "NoAssessmentsFound", null);
            var submissionIds = assessments
                .SelectMany(a => a.Submissions) 
                .Select(s => s.Id) // Then select the Id from each Submission
                .ToList();
            var submissions = assessments.SelectMany(a => a.Submissions)
                .Where(s => submissionIds.Contains(s.Id))
                .ToList();
            if (submissions?.Any() != true)
                throw new ApiException("No submissions found for the assessments.", (int)HttpStatusCode.NotFound, "NoSubmissionsFound", null);

            int totalAssessments = assessments.Count();
            int totalStudents = submissions.Select(s => s.StudentId).Distinct().Count();
            var totalSubmissionCount = submissions.Count;
            double averageScore = submissions.Average(s => s.TotalScore);
            double passRate = totalSubmissionCount > 0
               ? Math.Round((submissions.Count(s => s.TotalScore >= s.Assessment.RequiredPassingScore) * 100.0 / totalSubmissionCount),2)
               : 0;

            var dashboardDto = new InstructorDashboardOverview
            {
                TotalAssessments = totalAssessments,
                TotalStudents = totalStudents,
                AverageScore = averageScore,
                PassRate = passRate,
            };
        

            return new BaseResponse<InstructorDashboardOverview>
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

            return new BaseResponse<StudentDashBoardDto>
            {
                Message = "Student dashboard data retrieved successfully.",
                Status = true,
                Data = new StudentDashBoardDto
                {
                    AverageScore = averageScore,
                    CompletedAssessments = completedAssessmentsCount,
                    TotalAssessments = assessmentCount,
                    CompletionRate = completionRate,
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
                        .Count(ss => ss.Submissions.Any(sub => sub.TotalScore >= sub.Assessment.RequiredPassingScore))
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
        public async Task<BaseResponse<List<ScoreTrenddto>>> GetScoreTrendsAsync(Guid? instructorId, Guid? batchId, DateTime? date)
        {

            var targetDate = date ?? DateTime.UtcNow;
            var targetMonth = targetDate.Month;
            var targetYear = targetDate.Year;
            var monthStart = DateTime.SpecifyKind(new DateTime(targetYear, targetMonth, 1), DateTimeKind.Utc);
            var monthEnd = DateTime.SpecifyKind(monthStart.AddMonths(1), DateTimeKind.Utc);

                    var assessments = await _assessmentRepository.GetAllAsync(x =>
              (instructorId == null || x.InstructorId == instructorId.Value) &&
              (batchId == null || x.BatchAssessment.Any(ba => ba.BatchId == batchId.Value)) &&
              x.Submissions.Any(s => s.SubmittedAt >= monthStart && s.SubmittedAt < monthEnd)
          );
            // ✅ Determine total weeks in the target month
            var daysInMonth = DateTime.DaysInMonth(targetYear, targetMonth);
            var totalWeeks = (int)Math.Ceiling(daysInMonth / 7.0);

            if (assessments == null || !assessments.Any())
                return new BaseResponse<List<ScoreTrenddto>>
                {
                    Message = "Score trend data generated.",
                    Status = true,
                    Data = Enumerable.Range(1, totalWeeks)
                    .Select(week => new ScoreTrenddto
                    {
                        Label = $"Week {week}",
                        AverageScore = 0
                    })
                    .ToList()
                };
            

            var groupedTrends = assessments
                .SelectMany(a => a.Submissions.Where(s => s.SubmittedAt.Month == targetMonth && s.SubmittedAt.Year == targetYear))
                .GroupBy(s => GetWeekOfMonth(s.SubmittedAt)) // Week number
                .Select(g => new { Week = g.Key, AverageScore = Math.Round(g.Average(x => x.TotalScore), 2) })
                .ToDictionary(x => x.Week, x => x.AverageScore);

      
            var trends = Enumerable.Range(1, totalWeeks)
                .Select(week => new ScoreTrenddto
                {
                    Label = $"Week {week}",
                    AverageScore = groupedTrends.ContainsKey(week) ? groupedTrends[week] : 0
                })
                .ToList();

            return new BaseResponse<List<ScoreTrenddto>>
            {
                Message = "Score trend data generated.",
                Status = true,
                Data = trends
            };
        }

        public async Task<BaseResponse<List<AssessmentCreatedDto>>> GetAssessmentsCreatedTrendAsync(Guid? instructorId, Guid? batchId, DateTime? date)
        {
            var targetDate = date ?? DateTime.UtcNow;
            var targetMonth = targetDate.Month;
            var targetYear = targetDate.Year;
            var monthStart = DateTime.SpecifyKind(new DateTime(targetYear, targetMonth, 1), DateTimeKind.Utc);
            var monthEnd = DateTime.SpecifyKind(monthStart.AddMonths(1), DateTimeKind.Utc);// Exclusive end
                    var assessments = await _assessmentRepository.GetAllAsync(x =>
            (instructorId == null || x.InstructorId == instructorId.Value) &&
            (batchId == null || x.BatchAssessment.Any(ba => ba.BatchId == batchId.Value)) &&
            x.CreatedAt >= monthStart && x.CreatedAt < monthEnd
        );

            // Group assessments by week number (1 to 5)
            var groupedByWeek = assessments
                .GroupBy(a => GetWeekOfMonth(a.CreatedAt))
                .ToDictionary(g => g.Key, g => g.Count());

            // Always return week 1 to 5
            var result = Enumerable.Range(1, 5)
                .Select(week => new AssessmentCreatedDto
                {
                    Label = $"Week {week}",
                    Count = groupedByWeek.ContainsKey(week) ? groupedByWeek[week] : 0
                })
                .ToList();

            return new BaseResponse<List<AssessmentCreatedDto>>
            {
                Message = "Weekly assessment creation count for selected month.",
                Status = true,
                Data = result
            };
        }

        private int GetWeekOfMonth(DateTime date)
        {
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var dayOffset = (int)firstDayOfMonth.DayOfWeek;
            return ((date.Day + dayOffset - 1) / 7) + 1;
        }



        public async Task<BaseResponse<LeaderboardSummaryDto>> GetStudentDashboardLeaderboardSummaryAsync()
        {
            var userId = _currentUser.GetCurrentUserId();

            var student = await _userRepository.GetAsync(userId);
            if (student == null || student.BatchId == null)
            {
                return new BaseResponse<LeaderboardSummaryDto>
                {
                    Status = false,
                    Message = "Student not found or not assigned to a batch"
                };
            }

            var batchId = student.BatchId.Value;

            var leaderboard = await _leaderboardStore.GetLeaderBoardByBatchId(batchId);

            var topThree = leaderboard.Take(3).ToList();

            var studentIndex = leaderboard.FindIndex(l => l.Id == userId);
            LeaderboardDto? studentData = studentIndex != -1 ? leaderboard[studentIndex] : null;

            var response = new LeaderboardSummaryDto
            {
                TopThree = topThree,
                StudentRank = studentIndex != -1 ? studentIndex + 1 : (int?)null,
                CurrentStudent = studentData
            };

            return new BaseResponse<LeaderboardSummaryDto>
            {
                Status = true,
                Message = "Student leaderboard summary retrieved",
                Data = response
            };
        }


    }
}
