namespace Domain.Entities
{
    public class StudentAssessmentProgress
    {
        public Guid Id { get; set; }

        public Guid StudentId { get; set; }
        public Guid AssessmentId { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastSavedAt { get; set; }

        public TimeSpan ElapsedTime { get; set; } = TimeSpan.Zero; // Total accumulated time
        public DateTime? CurrentSessionStart { get; set; } // For active session tracking

        public ICollection<InProgressAnswer> Answers { get; set; } = new List<InProgressAnswer>();
    }


}
