using Domain.Entitties;
using Domain.Enum;

namespace Application.Dtos
{
    public class SubmissionDto
    {
        public Guid Id { get; set; }
        public Guid AssessmentId {  get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Title { get; set; }
        public int TotalScore { get;set; }
        public string FeedBack { get; set; }

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
        //public List<TestCaseDto> TestCases { get; set; } = new();
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
}
