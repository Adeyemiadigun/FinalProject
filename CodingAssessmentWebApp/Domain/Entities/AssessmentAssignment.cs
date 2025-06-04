using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entitties
{
    public class AssessmentAssignment
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid AssessmentId { get; set; }
        public Assessment Assessment { get; set; }
        public Guid StudentId { get; set; }
        public User Student { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public bool EmailSent { get; set; }
    }
}
