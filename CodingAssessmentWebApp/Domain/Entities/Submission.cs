namespace Domain.Entitties
{
    public class Submission
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid AssessmentId {  get; set; }
        public Assessment Assessment { get; set; }
        public Guid StudentId { get; set; }
        public User Student { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public short TotalScore { get; set; }
        public string FeedBack {  get; set; }
        public bool IsAutoSubmitted { get; set; }
        public ICollection<AnswerSubmission> AnswerSubmissions { get; set; } = [];
    }
}
