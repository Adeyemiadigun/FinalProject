using Domain.Enum;

namespace Application.Dtos
{
    public class AssessmentDto
    {
        public Guid Id { get; init; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TechnologyStack { get; set; }
        public string InstructorName { get; set; }
        public int DurationInMinutes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public double PassingScore { get; set; }
        public double TotalMarks { get; set; }
        public bool Submitted { get; set; }
        public string Status { get; set; }
    }
    public class StudentAssessmentDetail
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public double Score { get; set; }
        public bool Status { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? SubmittedDate { get; set; } // Nullable for not submitted assessments

    }
    public class AssessmentMetrics
    {
        public double  AvgScore { get; set; }
        public double PassRate { get; set; }
        public int TotalSubmissions { get; set; }
        public double CompletionRate { get; set; }

    }
    public class BatchPerformance
    {
        public string BatchName { get; set; }
        public double AverageScore { get; set; } 
    }
    public class BatchAssessmentsOverview
    {
        public Guid Id { get; init; } 
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalStudents { get; set; }
        public int Submissions { get; set; }
        public double AvgScore { get; set; }
}
    public class CreateAssessmentRequestModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public TechnologyStack TechnologyStack { get; set; }
        public int DurationInMinutes { get; set; }
        public List<Guid> BatchIds { get; set; } = [];
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double PassingScore { get; set; }
    }
    public class AssignStudentsModel
    {
        public List<Guid> StudentIds { get; set; } = [];
    }
    public class UpdateAssessmentRequestModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public TechnologyStack TechnologyStack { get; set; }
        public int DurationInMinutes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double PassingScore { get; set; }
    }
    public class InstructorAssessmentDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string TechnologyStack { get; set; }
        public int DurationInMinutes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> BatchNames { get; set; }
        public List<StudentScoreDto> Students { get; set; }

        public List<BatchPerformanceDto> BatchPerformance { get; set; } = new();
    }
    public class StudentScoreDto
    {
        public Guid StudentId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Batch { get; set; }
        public double Score { get; set; }
    }
    public class AssessmentScoreDistribution
    {
        public string Cap { get;set; }
        public int Count { get; set; }

    }
    public class StudentAssessmeentPerformance
    {
        public Guid StudentId { get; set; }
        public string Name { get; set; }
        public string Batch { get; set; }
        public double Score { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string Status { get; set; }
    }

    public class BatchPerformanceDto
    {
        public string BatchName { get; set; }
        public double AverageScore { get; set; }
    }
    public class InstructorAssessmentPerformanceDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int StudentCount { get; set; }
        public double AvgScore { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class AdminAssessmentDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string InstructorName { get; set; }
        public List<BatchDto> Batches { get; set; } = new();
        public string TechnologyStack { get; set; }
        public int DurationInMinutes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double PassingScore { get; set; }
        public double TotalScore { get; set; }
        public string Status { get; set; } 
    }

    public class AssessmentHistoryItemDto
    {
        public Guid AssessmentId { get; set; }
        public string Title { get; set; }
        public double Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string FeedBack { get; set; }
    }

    public class AssessmentOverviewDto
    {
        public List<string> AssignedBatches { get; set; }
        public int TotalAssignedStudents { get; set; }
        public int SubmittedCount { get; set; }
        public int NotSubmittedCount { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
    }
    public class GroupedStudentDto
    {
        public Guid StudentId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string BatchName { get; set; }
        public double? TotalScore { get; set; } // null if not submitted
    }


}
