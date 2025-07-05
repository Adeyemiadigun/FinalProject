using Domain.Entitties;

namespace Domain.Entities
{
    public class BatchAssessment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid AssessmentId { get; set; }
        public Assessment Assessment { get; set; }

        public Guid BatchId { get; set; }
        public Batch Batch { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }

}
