using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Domain.Enum;

namespace Application.Dtos
{
    public class AiQuestionGenerationDto
    {

    }
    public class OpenAIPayload
    {
        
        public string Model { get; set; }

     
        public List<Message> Messages { get; set; }

      
        public double Temperature { get; set; } = 0.7;
    }

    public class Message
    {
    
        public string Role { get; set; }

        public string Content { get; set; }
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
        [JsonPropertyName("questionText")]
        public string QuestionText { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("questionType")]
        public QuestionType QuestionType { get; set; } = QuestionType.MCQ;

        [JsonPropertyName("options")]
        public List<OptionDto> Options { get; set; }
    }

    public class AIObjectiveResponseDto
    {
        [JsonPropertyName("questionText")]
        public string QuestionText { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("questionType")]
        public QuestionType QuestionType { get; set; } = QuestionType.Objective;

        [JsonPropertyName("answerText")]
        public string AnswerText { get; set; }
    }

    public class AICodingResponseDto
    {
        [JsonPropertyName("questionText")]
        public string QuestionText { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("questionType")]
        public QuestionType QuestionType { get; set; } = QuestionType.Coding;

        [JsonPropertyName("testCases")]
        public List<CreateTestCaseDto> TestCases { get; set; } = new();
    }

}
