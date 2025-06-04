using System.ComponentModel.DataAnnotations;
using Domain.Entitties;
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
        public QuestionType QuestionType { get; set; }
        public List<CreateOptionDto> Options { get; set; }
    }
    public class AIObjectiveResponseDto
    {
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public string AnswerText { get; set; }
        public CreateAnswerDto Answer { get; set; }
    }
    public class AICodingResponseDto
    {
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public List<CreateTestCaseDto> TestCases { get; set; } = [];
    }
    
}
