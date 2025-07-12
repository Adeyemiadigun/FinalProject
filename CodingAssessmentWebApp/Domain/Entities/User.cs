using Domain.Entities;
using Domain.Enum;

namespace Domain.Entitties
{
    public class User
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public Role Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public Guid? BatchId { get; set; } 
        public Batch? Batch { get; set; }
        public ICollection<Assessment> Assessments { get; set; } = [];
        public ICollection<Submission> Submissions { get; set; } = [];
        public ICollection<AssessmentAssignment> AssessmentAssignments { get; set; } = [];

    }
}
