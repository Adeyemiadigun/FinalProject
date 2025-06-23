using Domain.Entitties;

namespace Domain.Entities
{
    public class SelectedOption
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AnswerSubmissionId { get; set; }
        public Guid OptionId { get; set; }
        public Option Option { get; set; }

        public AnswerSubmission AnswerSubmission { get; set; }
    }

}
