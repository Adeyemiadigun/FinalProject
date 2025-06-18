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
                QuestionType.MCQ => $"Generate a {request.Difficulty} MCQ in {request.TechnologyStack} with 4 options and correct answer in JSON.",
                QuestionType.Objective => $"Generate a {request.Difficulty} objective question in {request.TechnologyStack} with a one-line answer in JSON.",
                QuestionType.Coding => $"Generate a {request.Difficulty} coding challenge in {request.TechnologyStack} with description, input/output, test case, explanation in JSON.",
                _ => throw new ArgumentException("Invalid question type")
            };
        }
    }

}
