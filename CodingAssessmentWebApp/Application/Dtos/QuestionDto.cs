using Domain.Entitties;
using Domain.Enum;

namespace Application.Dtos
{
     public class QuestionDto
    {
        public Guid Id { get; set; }
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public int Marks { get; set; }
        public int Order { get; set; }
        public List<OptionDto> Options { get; set; }
        public List<CreateTestCaseDto> TestCases { get; set; } = [];
        public CreateAnswerDto Answer { get; set; }
    }
    public class UpdateQuestionDto
    {
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public int Marks { get; set; }
        public List<OptionDto> Options { get; set; }
        public List<CreateTestCaseDto> TestCases { get; set; } = [];
        public CreateAnswerDto Answer { get; set; }
    }
    public class CreateQuestionRequestModel
    {
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public int Marks { get; set; }
        public int Order { get; set; }
        public List<OptionDto> Options { get; set; }
        public List<CreateTestCaseDto> TestCases { get; set; } = [];
        public CreateAnswerDto Answer { get; set; }
    }
    public class OptionDto
    {
        public string OptionText { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class CreateTestCaseDto
    {
        public string Input { get; set; }
        public string ExpectedOutput { get; set; }
        public double Weight { get; set; }
    }
    public class CreateAnswerDto
    {
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }
        public string AnswerText { get; set; }
    }
}

