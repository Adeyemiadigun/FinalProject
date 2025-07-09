namespace Domain.Entities
{
    public class InProgressAnswer
    {
        public Guid Id { get; set; }

        public Guid QuestionId { get; set; }
        public string? AnswerText { get; set; } // For subjective answers or coding
        public ICollection<InProgressSelectedOption> SelectedOptions { get; set; } = new List<InProgressSelectedOption>();

        public Guid StudentAssessmentProgressId { get; set; }
        public StudentAssessmentProgress StudentAssessmentProgress { get; set; }
    }


}
