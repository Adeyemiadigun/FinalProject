using Application.Dtos;

namespace Application.Interfaces.ExternalServices.AIProviderStrategy
{
    public interface IAIProviderStrategyFactory
    {
        IAIProviderStrategy GetProviderStrategy(AiQuestionGenerationRequestDto request);
    }
}
