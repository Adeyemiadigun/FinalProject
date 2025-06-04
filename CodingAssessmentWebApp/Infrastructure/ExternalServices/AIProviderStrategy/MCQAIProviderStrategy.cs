using System.Net.Http.Headers;
using Application.Exceptions;
using Application.Interfaces.ExternalServices.AIProviderStrategy;
using Infrastructure.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.ExternalServices.AIProviderStrategy
{
    public class McqAIProviderStrategy : IAIProviderStrategy
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly HuggingFaceSettings _huggingFaceSettings;
        public McqAIProviderStrategy(HttpClient httpClient, IOptions<HuggingFaceSettings> option)
        {
            _httpClient = httpClient;
            _huggingFaceSettings = option.Value;
            _apiKey = _huggingFaceSettings.ApiToken;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> GenerateTextAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ApiException("Prompt cannot be null or empty.", 400, nameof(prompt), null);
            }
            var response = await _httpClient.GetAsync(prompt);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            else
            {
                throw new ApiException($"Error generating text: {response.ReasonPhrase}", (int)response.StatusCode, response.ReasonPhrase, null);
            }
        }
    }
}
