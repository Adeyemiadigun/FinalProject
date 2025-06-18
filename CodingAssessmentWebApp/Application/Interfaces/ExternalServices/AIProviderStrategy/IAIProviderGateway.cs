using Application.Dtos;

namespace Application.Interfaces.ExternalServices.AIProviderStrategy
{
    public interface IAIProviderGateway
    {

        Task<string> GenerateTextAsync(AiQuestionGenerationRequestDto request, string prompt);
    }
}
