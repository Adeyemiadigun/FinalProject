using System.Net;
using System.Text.Json;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.ExternalServices.AIProviderStrategy;
using Application.Interfaces.Services;
using Domain.Enum;

namespace Application.Services
{
    public class AIQuestionService : IAIQuestionService
    {
        private readonly IAIProviderGateway _aiProviderGateway;
       
        public AIQuestionService(IAIProviderGateway gateway)
        {
            _aiProviderGateway = gateway;
        }
        public async Task<BaseResponse<object>> GenerateQuestionAsync(AiQuestionGenerationRequestDto request)
        {
            string prompt = BuildPrompt(request);

            var aiResponse = await _aiProviderGateway.GenerateTextAsync(request, prompt);
            if (string.IsNullOrEmpty(aiResponse))
            {
                throw new ApiException("AI response is empty or null", (int)HttpStatusCode.UnprocessableEntity, "EmptyResponse", null);
            }
            try
            {
                object deserializedResponse = request.QuestionType switch
                {
                    QuestionType.MCQ => JsonSerializer.Deserialize<AIMCQResponseDto>(aiResponse),
                    QuestionType.Objective => JsonSerializer.Deserialize<AIObjectiveResponseDto>(aiResponse),
                    QuestionType.Coding => JsonSerializer.Deserialize<AICodingResponseDto>(aiResponse),
                    _ => throw new ArgumentException("Invalid question type")
                };

                return new BaseResponse<object>
                {
                    Data = deserializedResponse,
                    Status = true,
                    Message = "Question generated successfully"
                };
            }
            catch (Exception ex)
            {
                throw new ApiException("Error deserializing AI response", (int)HttpStatusCode.InternalServerError, "DeserializationError", ex);
            }
        }

        private string BuildPrompt(AiQuestionGenerationRequestDto request)
        {
            return request.QuestionType switch
            {
                QuestionType.MCQ =>
                    $"Generate a {request.Difficulty} multiple-choice question (MCQ) in the {request.TechnologyStack} domain. " +
                    "Format the response strictly in this JSON structure: " +
                    "{{ " +
                    "\"questionText\": \"<question>\", " +
                    "\"questionType\": \"MCQ\", " +
                    "\"options\": [" +
                        "{ \"optionText\": \"Option A\", \"isCorrect\": false }, " +
                        "{ \"optionText\": \"Option B\", \"isCorrect\": true }, " +
                        "{ \"optionText\": \"Option C\", \"isCorrect\": false }, " +
                        "{ \"optionText\": \"Option D\", \"isCorrect\": false } " +
                    "] " +
                    "}}",

                QuestionType.Objective =>
                    $"Generate a {request.Difficulty} objective question in {request.TechnologyStack} that requires a short text-based answer. " +
                    "Return it strictly in this JSON format: " +
                    "{{ " +
                    "\"questionText\": \"<question>\", " +
                    "\"questionType\": \"Objective\", " +
                    "\"answerText\": \"<short-answer>\" " +
                    "}}",

                QuestionType.Coding =>
                    $"Generate a {request.Difficulty} coding challenge in {request.TechnologyStack}. " +
                    "It should include a concise problem statement, and a list of test cases with input, expected output, and weight. " +
                    "Return it strictly in this JSON format: " +
                    "{{ " +
                    "\"questionText\": \"<problem description>\", " +
                    "\"questionType\": \"Coding\", " +
                    "\"testCases\": [" +
                        "{ \"input\": \"input string here\", \"expectedOutput\": \"expected output here\", \"weight\": 5 }, " +
                        "{ \"input\": \"another input\", \"expectedOutput\": \"another output\", \"weight\": 5 } " +
                    "] " +
                    "}}",

                _ => throw new ArgumentException("Invalid question type")
            };

        }

    }

}
