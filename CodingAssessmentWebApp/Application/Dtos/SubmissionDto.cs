using Domain.Entitties;

namespace Application.Dtos
{
    public class SubmissionDto
    {
    }
    public class AnswerSubmissionDto
    {
        public Guid AssessmentId { get; set; }
        public List<QuestionAnswers> Answers { get; set; } 
    }
    public class QuestionAnswers
    {
        public Guid QuestionId { get; set; }
        public string SubmittedAnswer { get; set; }
        public List<Guid> SelectedOptionIds { get; set; } = [];
    }
}
