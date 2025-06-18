using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.ExternalServices.AIProviderStrategy;

namespace Infrastructure.ExternalServices.AIProviderStrategy
{
    public class AIProviderStrategyFactory : IAIProviderStrategyFactory
    {
        private readonly IEnumerable<IAIProviderStrategy> _aiprovider;

        public AIProviderStrategyFactory(IEnumerable<IAIProviderStrategy> aiprovider)
        {
            _aiprovider = aiprovider;
        }

        public IAIProviderStrategy GetProviderStrategy(AiQuestionGenerationRequestDto request)
        {
            return _aiprovider.FirstOrDefault(x => x.QuestionType == request.QuestionType)
                ?? throw new ApiException(
                    message: "No matching AI provider strategy found.",
                    statusCode: 404,
                    errorCode: "AI_PROVIDER_NOT_FOUND",
                    error: new { request.QuestionType });
        }
    }
}
