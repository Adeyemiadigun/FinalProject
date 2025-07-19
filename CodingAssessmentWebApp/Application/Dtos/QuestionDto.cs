using System.Text.Json.Serialization;
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
        [JsonPropertyName("optionText")]
        public string OptionText { get; set; }

        [JsonPropertyName("isCorrect")]
        public bool IsCorrect { get; set; }
    }

    public class CreateTestCaseDto
    {
        [JsonPropertyName("input")]
        public string Input { get; set; }
        [JsonPropertyName("expectedOutput")]
        public string ExpectedOutput { get; set; }
        [JsonPropertyName("weight")]
        public double Weight { get; set; }
    }
    public class CreateAnswerDto
    {
        public string AnswerText { get; set; }
    }
}

