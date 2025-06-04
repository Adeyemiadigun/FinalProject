using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.ExternalServices.AIProviderStrategy;
using Application.Interfaces.Services;
using Domain.Enum;

namespace Application.Services
{
    public class AIQuestionService : IAIQuestionService
    {
        private readonly IAIProviderStrategy _aiProvider;

        public AIQuestionService(IAIProviderStrategy aiProvider)
        {
            _aiProvider = aiProvider;
        }
        public async Task<object> GenerateQuestionAsync(AiQuestionGenerationRequestDto request)
        {
            string prompt = BuildPrompt(request);
            string aiResponse = await _aiProvider.GenerateTextAsync(prompt);
            if (string.IsNullOrEmpty(aiResponse) || aiResponse is null)
            {
                throw new ApiException("AI response is empty or null", (int)HttpStatusCode.UnprocessableEntity, "EmptyResponse", null);
            }
            return request.QuestionType switch
            {
                QuestionType.MCQ => JsonSerializer.Deserialize<AIMCQResponseDto>(aiResponse),
                QuestionType.Objective => JsonSerializer.Deserialize<AIObjectiveResponseDto>(aiResponse),
                QuestionType.Coding => JsonSerializer.Deserialize<AICodingResponseDto>(aiResponse),
                _ => throw new ArgumentException("Invalid question type")
            };
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
