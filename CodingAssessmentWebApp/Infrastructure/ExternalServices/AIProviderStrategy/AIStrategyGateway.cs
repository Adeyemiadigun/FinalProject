using System.Net.Http.Headers;
using System.Text;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.ExternalServices.AIProviderStrategy;
using Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Infrastructure.ExternalServices.AIProviderStrategy
{
    public class AIStrategyGateway : IAIProviderGateway
    {
        private readonly HttpClient _httpClient;
        private readonly OpenRouter _openRouter;
        private readonly IPayloadBuider _payloadBuilder;
        public AIStrategyGateway(HttpClient httpClient, IPayloadBuider payloadBuider, IOptions<OpenRouter> openRouter)
        {
            _httpClient = httpClient;
            _payloadBuilder = payloadBuider;
            _openRouter = openRouter.Value;
        
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _openRouter.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
        }
        public async Task<string> GenerateTextAsync(AiQuestionGenerationRequestDto request, string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ApiException("Prompt cannot be null or empty.", 400, nameof(prompt), null);
            }
            const int maxRetries = 3;


            string url = _openRouter.Url;
            var payload = _payloadBuilder.BuildPayload(prompt, "questionGen");
                if (payload is null)
                    throw new ApiException("Payload cannot be null or empty.", 400, nameof(payload), null);

               

                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                 var response = await _httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var modelResponse = await response.Content.ReadAsStringAsync();
                        return modelResponse;
                    }
                           
            return null!;
        }

    }
}
