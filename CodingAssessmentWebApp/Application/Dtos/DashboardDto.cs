using System;
using System.Numerics;

namespace Application.Dtos
{
    public class StudentDashboardSummaryDto
    {
        public int TotalAssessments { get; set; }
        public double AverageScore { get; set; }
        public double HighestScore { get; set; }
        public double CompletionRate { get; set; }
    }
    public class StudentDashBoardDto
    {
        public int TotalAssessments { get; set; }
        public int ActiveAssessments { get; set; }
        public int CompletedAssessments { get; set; }
        public double AverageScore { get; set; }
        public double CompletionRate { get; set; }

    }
    public class StudentAssessmentDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public string Status { get; set; } // Ongoing, Upcoming, Completed
        public DateTime StartedDate { get; set; } // for ongoing only
        public int Score { get; set; } // for history
    }

    public class StudentScoreTrendDto
    {
        public List<string> Labels { get; set; }
        public List<double> Scores { get; set; }
    }

    public class StudentRankingDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Rank { get; set; }
        public double Score { get; set; }
    }
    public class AdminDashBoardOverview
    {

        public int TotalAssessments { get; set; }
        public int ActiveAssessments { get; set; }
        public int TotalStudents { get; set; }
        public double AverageScore { get; set; }
        public double CompletionRate { get; set; }
        public List<TopStudentDto> TopStudents { get; set; }
        public List<LowestPerformingStudent> LowestStudents { get; set; }
        public int TotalBatches { get; set; }
    }
    public class InstructorDashboardOverview
    {
        public int TotalAssessments { get; set; }
        public int TotalStudents { get; set; }
        public double AverageScore { get; set; }
        public double PassRate { get; set; }
    }

    public class TopStudentDto
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public double AverageScore { get; set; }
    }
    public class LowestPerformingStudent
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public double AverageScore { get; set; }
    }

    public class WeakAreaDto
    {
        public string Topic { get; set; }
        public double AverageScore { get; set; }
    }
    public class BatchAnalytics
    {

        public Guid Id { get; set; }
        public string Name { get; set; }
        public int StudentCount { get; set; }
        public double AverageScore { get; set; }
        public double PassRate { get; set; }
    }
    public class BatchDetails
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double AverageScore { get; set; }
        public double PassRate { get; set; }
        public double CompletionRate { get; set; }
        public string AverageCompletionTime { get; set; }
        public int TotalStudents { get; set; }
        public int TotalAssessments { get; set; }
    }
    
    public class AssessmentPerformanceDto
    {
        public Guid Id { get; set; }
        public string AssessmentTitle { get; set; }
        public double AverageScore { get; set; }
    }
    public class AssessmentMetricsDto
    {
        public int TotalAssessments { get; set; }
        public int ActiveAssessments { get; set; }
        public double AverageScore { get; set; }
        public double PassRate { get; set; }
        public double CompletionRate { get; set; }
    }
    public class QuestionTypeMetrics
    {
        public string QuestionType { get; set; }
        public double AverageScore { get; set; }
    }
    public class ScoreTrenddto
    {
        public string Label { get; set; }
        public double AverageScore { get; set; }
    }

    public class AssessmentCreatedDto
    {
        public string Label { get; set; }
        public int Count { get; set; }
    }
    public class BatchPerformanceTrendDto
    {
        public List<string> Labels { get; set; }
        public List<double> Scores { get; set; }
    }

}
