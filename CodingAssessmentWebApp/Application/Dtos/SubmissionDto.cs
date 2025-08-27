using Domain.Entities;
using Domain.Entitties;
using Domain.Enum;

namespace Application.Dtos
{
    public class SubmissionDto
    {
        public Guid Id { get; set; }
        public Guid AssessmentId {  get; set; }
        public string AssessmentTitle { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Title { get; set; }
        public double PassingPercentage { get; set; }
        public double TotalMarks { get; set; }
        public int TotalScore { get;set; }
        public string FeedBack { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }

        public List<SubmittedAnswerDto> SubmittedAnswers { get; set; } = new List<SubmittedAnswerDto>();
    }
    public class SubmittedAnswerDto
    {
        public Guid QuestionId { get; set; }
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public string SubmittedAnswer { get; set; }
        public int Order { get; set; }
        public bool IsCorrect { get; set; }
        public int Score { get; set; }
        public List<OptionDto> Options { get; set; } = new();
        public List<OptionDto> SelectedOptions { get; set; } = new List<OptionDto>();
        public List<TestCaseResultDto> TestCases { get; set; } = new List<TestCaseResultDto>();
    }
    public class TestCaseResultDto
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
    public class AnswerSubmissionDto
    {
        public List<QuestionAnswers> Answers { get; set; } 
    }
    public class QuestionAnswers
    {
        public Guid QuestionId { get; set; }
        public string SubmittedAnswer { get; set; }
        public List<Guid> SelectedOptionIds { get; set; } = [];
    }
    public class SubmissionsDto
    {
        public Guid Id { get; set; }
        public Guid AssessmentId { get; set; }
        public string AssessmentTitle { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Title { get; set; }
        public int TotalScore { get; set; }
        public string FeedBack { get; set; }
        public DateTime AssignedDate { get; set; }
        public string StudentName { get; set; }
    }
    public class SubmissionStatsDto
    {
        public string AssessmentTitle { get; set; }
        public int TotalAssigned { get; set; }
        public int TotalSubmitted { get; set; }
    }
}
