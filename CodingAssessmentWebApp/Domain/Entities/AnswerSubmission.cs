using Domain.Entities;

namespace Domain.Entitties
{
    public class AnswerSubmission
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid SubmissionId { get; set; }
        public Submission Submission { get; set; }
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }
        public string SubmittedAnswer { get; set; }
        public short Score { get; set; }
        public bool IsCorrect { get; set; }
        public List<TestCaseResult> TestCaseResults { get; set; } = [];
        public ICollection<SelectedOption> SelectedOptions { get; set; } = [];


    }
}
