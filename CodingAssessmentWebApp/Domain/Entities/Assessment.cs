using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities;
using Domain.Enum;

namespace Domain.Entitties
{
    public class Assessment
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Description { get; set; }
        public TechnologyStack TechnologyStack { get; set; }
        public int DurationInMinutes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AssessmentStatus Status { get; set; }
        public Guid InstructorId { get; set; }
        public User Instructor { get; set; }
        public ICollection<BatchAssessment> BatchAssessment { get; set; } = [];
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public double PassingPercentage { get; set; } // e.g., 60.0 for 60%

        [NotMapped]
        public double TotalAvailableMarks => Questions?.Sum(q => q.Marks) ?? 0;

        [NotMapped]
        public double RequiredPassingScore => TotalAvailableMarks * (PassingPercentage / 100.0);

        public ICollection<Question> Questions { get; set; } = [];
        public ICollection<Submission> Submissions { get; set; } = [];
        public ICollection<AssessmentAssignment> AssessmentAssignments { get; set; } = [];
    }
}
