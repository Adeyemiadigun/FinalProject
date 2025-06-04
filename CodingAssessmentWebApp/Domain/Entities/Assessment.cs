using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entitties
{
    public class Assessment
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Description { get; set; }
        public string TechnologyStack { get; set; }
        public int DurationInMinutes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid InstructorId { get; set; }
        public User Instructor { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public double PassingScore { get; set; } 
        public ICollection<Question> Questions { get; set; } = [];
        public ICollection<Submission> Submissions { get; set; } = [];
        public ICollection<AssessmentAssignment> AssessmentAssignments { get; set; } = [];
    }
}
