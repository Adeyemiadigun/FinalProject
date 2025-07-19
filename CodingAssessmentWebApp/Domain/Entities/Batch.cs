using Domain.Entitties;

namespace Domain.Entities
{
    public class Batch
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public short BatchNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<User> Students { get; set; } = new List<User>();
        public ICollection<BatchAssessment> AssessmentAssignments { get; set; } = [];
        public Batch()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        public void Update()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
