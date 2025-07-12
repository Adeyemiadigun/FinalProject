using Domain.Entitties;
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
        public bool Submitted { get; set; }
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
        public string TechnologyStack { get; set; }
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
        public string TechnologyStack { get; set; }
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
}
