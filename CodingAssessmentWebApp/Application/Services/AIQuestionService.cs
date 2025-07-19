using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.ExternalServices.AIProviderStrategy;
using Application.Interfaces.Services;
using Domain.Enum;
using System.Text;
using static System.Net.WebRequestMethods;

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

            var aiRawResponse = await _aiProviderGateway.GenerateTextAsync(request, prompt);

            if (string.IsNullOrWhiteSpace(aiRawResponse))
            {
                throw new ApiException("AI response is empty or null", (int)HttpStatusCode.UnprocessableEntity, "EmptyResponse", null);
            }

            try
            {
                // STEP 1: Extract content from OpenRouter-style response
                using var doc = JsonDocument.Parse(aiRawResponse);
                var contentString = doc
                    .RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(contentString))
                    throw new ApiException("Empty content returned by AI", 422, "EMPTY_AI_CONTENT", null);

                // STEP 2: Clean and deserialize actual content
                var cleaned = contentString.Trim();

                // Handle any potential leading/trailing quotes from the JSON string
                if (cleaned.StartsWith("\"") && cleaned.EndsWith("\""))
                {
                    cleaned = JsonSerializer.Deserialize<string>(cleaned); // Unescape the string
                }

                object deserializedResponse = request.QuestionType switch
                {
                    QuestionType.MCQ => JsonSerializer.Deserialize<AIMCQResponseDto>(cleaned),
                    QuestionType.Objective => JsonSerializer.Deserialize<AIObjectiveResponseDto>(cleaned),
                    QuestionType.Coding => JsonSerializer.Deserialize<AICodingResponseDto>(cleaned),
                    _ => throw new ArgumentException("Invalid question type")
                };

                return new BaseResponse<object>
                {
                    Status = true,
                    Message = "Question generated successfully",
                    Data = deserializedResponse
                };
            }
            catch (JsonException ex)
            {
                throw new ApiException("Deserialization failed", 500, "DeserializationError", ex);
            }
        }
        private string BuildPrompt(AiQuestionGenerationRequestDto request)
        {
            return request.QuestionType switch
            {
                QuestionType.MCQ =>
                    $"Generate a {request.Difficulty} multiple-choice question on {request.TechnologyStack}. in the topic {request.Topic}" +
                    "Respond ONLY in this JSON format. Replace <question> and options with actual content:\n\n" +
                    "{{\n" +
                    "  \"questionText\": \"<question>\",\n" +
                    "  \"questionType\": \"MCQ\",\n" +
                    "  \"options\": [\n" +
                    "    {{ \"optionText\": \"\", \"isCorrect\": false }},\n" +
                    "    {{ \"optionText\": \"\", \"isCorrect\": true }},\n" +
                    "    {{ \"optionText\": \"\", \"isCorrect\": false }},\n" +
                    "    {{ \"optionText\": \"\", \"isCorrect\": false }}\n" +
                    "  ]\n" +
                    "}}",

                QuestionType.Objective =>
                    $"Generate a {request.Difficulty} objective question in {request.TechnologyStack}.in the topic {request.Topic} " +
                    " \"Respond ONLY in this JSON format. Replace <question> and <short-answer> with actual content:\n\n" +
                    "{{\n" +
                    "  \"questionText\": \"<question>\",\n" +
                    "  \"questionType\": \"Objective\",\n" +
                    "  \"answerText\": \"<short-answer>\"\n" +
                    "}}",

                QuestionType.Coding =>
                    $"Generate a {request.Difficulty} coding challenge in {request.TechnologyStack}.in the topic {request.Topic} " +
                    "Include a clear problem description and test cases. Return ONLY in this JSON format with real values Respond ONLY in this JSON format. Replace <problem description> and testcases with actual content:\n\n" +
                    "{{\n" +
                    "  \"questionText\": \"<problem description>\",\n" +
                    "  \"questionType\": \"Coding\",\n" +
                    "  \"testCases\": [\n" +
                    "    {{ \"input\": \"1,2,3\", \"expectedOutput\": \"6\", \"weight\": 5 }},\n" +
                    "    {{ \"input\": \"5,5\", \"expectedOutput\": \"10\", \"weight\": 5 }}\n" +
                    "  ]\n" +
                    "}}",

                _ => throw new ArgumentException("Invalid question type")
            };
        }


    }

}
