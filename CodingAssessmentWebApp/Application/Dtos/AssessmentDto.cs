using Domain.Entitties;
using Domain.Enum;

namespace Application.Dtos
{
    public class AssessmentDto
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Description { get; set; }
        public string TechnologyStack { get; set; }
        public int DurationInMinutes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public double PassingScore { get; set; }
    }
    public class CreateAssessmentRequestModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string TechnologyStack { get; set; }
        public int DurationInMinutes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double PassingScore { get; set; }
        public List<Guid> AssignedStudentIds { get; set; } = [];
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
}
   