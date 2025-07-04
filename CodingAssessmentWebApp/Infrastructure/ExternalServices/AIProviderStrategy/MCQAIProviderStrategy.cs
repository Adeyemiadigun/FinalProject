﻿using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Exceptions;
using Application.Interfaces.ExternalServices.AIProviderStrategy;
using Domain.Enum;
using Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Infrastructure.ExternalServices.AIProviderStrategy
{
    public class McqAIProviderStrategy : IAIProviderStrategy
    {
        public QuestionType QuestionType => QuestionType.MCQ;
        private readonly HttpClient _httpClient;
        private readonly AISettings aISettings;
        private readonly IPayloadBuider _payloadBuilder;
        public McqAIProviderStrategy(HttpClient httpClient, IOptions<AISettings> option,IPayloadBuider payloadBuider)
        {
            _httpClient = httpClient;
            aISettings = option.Value;
            _payloadBuilder = payloadBuider;
        }

        public async Task<string> GenerateTextAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ApiException("Prompt cannot be null or empty.", 400, nameof(prompt), null);
            }
            var models = aISettings.QuestionTypeProviders["MCQ"];
            foreach( var model in models )
            {
                var payload = _payloadBuilder.BuildPayload(model.Name, prompt);
                if (payload is null)
                    continue;

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", model.ApiKey);
                var response = await _httpClient.PostAsJsonAsync(model.ApiUrl, payload);
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
