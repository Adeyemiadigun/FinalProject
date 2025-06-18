using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.ExternalServices.AIProviderStrategy;
using Application.Services;
using Infrastructure.Configurations;
using Domain.Enum;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Infrastructure.ExternalServices.AIProviderStrategy
{
    public class AIStrategyGateway : IAIProviderGateway
    {
        private readonly HttpClient _httpClient;
        private readonly AISettings aISettings;
        private readonly IPayloadBuider _payloadBuilder;
        public AIStrategyGateway(HttpClient httpClient, IOptions<AISettings> option, IPayloadBuider payloadBuider)
        {
            _httpClient = httpClient;
            aISettings = option.Value;
            _payloadBuilder = payloadBuider;
        }
        public async Task<string> GenerateTextAsync(AiQuestionGenerationRequestDto request, string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ApiException("Prompt cannot be null or empty.", 400, nameof(prompt), null);
            }
            var models = aISettings.QuestionTypeProviders[request.QuestionType.ToString()];
            foreach (var model in models)
            {
                var payload = _payloadBuilder.BuildPayload(model.Name, prompt);
                if (payload is null)
                    continue;
                string finalUrl = string.Empty;
                    if (model.AuthType == "Bearer")
                    {
                        _httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", model.ApiKey);
                        finalUrl = model.ApiUrl;
                    }
                    if (model.AuthType == "Query")
                    {
                        _httpClient.DefaultRequestHeaders.Authorization = null;
                        finalUrl = $"{model.ApiUrl}?key={model.ApiKey}";
                    }
                    
                var response = await _httpClient.PostAsJsonAsync(finalUrl, payload);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return content;
                }
            }
            return null!;
        }
    }
}
