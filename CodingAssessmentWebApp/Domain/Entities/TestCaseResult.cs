using Domain.Entitties;

namespace Domain.Entities
{
    public class TestCaseResult
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AnswerSubmissionId { get; set; }
        public AnswerSubmission AnswerSubmission { get; set; }
        public string Input { get; set; }
        public string ExpectedOutput { get; set; }
        public string ActualOutput { get; set; }
        public bool Passed { get; set; }
        public double EarnedWeight { get; set; }
    }
}
