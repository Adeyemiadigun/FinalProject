using System.ComponentModel.DataAnnotations;
using Domain.Enum;

namespace Application.Dtos
{
    public class AiQuestionGenerationDto
    {

    }
    public class AiQuestionGenerationRequestDto
    {
        [Required]
        public QuestionType QuestionType { get; set; }
        [Required]
        public string TechnologyStack { get; set; }
        [Required]
        public string Difficulty { get; set; }
        [Required]
        public string Topic { get; set; }
    }
    public class AIMCQResponseDto
    {
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; } = QuestionType.MCQ;
        public List<OptionDto> Options { get; set; }
    }
    public class AIObjectiveResponseDto
    {
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; } = QuestionType.Objective;
        public string AnswerText { get; set; }
        public CreateAnswerDto Answer { get; set; }
    }
    public class AICodingResponseDto
    {
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; } = QuestionType.Coding; 
        public List<CreateTestCaseDto> TestCases { get; set; } = [];
    }
    
}
