using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces.ExternalServices.AIProviderStrategy;
using Domain.Enum;
using Infrastructure.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.ExternalServices.AIProviderStrategy
{
    public class AIProviderStrategyFactory : IAIProviderStrategyFactory
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<HuggingFaceSettings> _huggingFaceSettings;

        public AIProviderStrategyFactory(HttpClient httpClient, IOptions<HuggingFaceSettings> huggingFaceSettings)
        {
            _httpClient = httpClient;
            _huggingFaceSettings = huggingFaceSettings;
        }

        public IAIProviderStrategy GetProviderStrategy(AiQuestionGenerationRequestDto request)
        {
            return request.QuestionType switch
            {
                QuestionType.MCQ => new McqAIProviderStrategy(_httpClient, _huggingFaceSettings),
                _ => throw new NotSupportedException($"Question type {request.QuestionType} is not supported.")
            };
        }
    }
}
