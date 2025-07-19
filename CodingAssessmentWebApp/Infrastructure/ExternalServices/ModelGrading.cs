using Application.Dtos;
using Application.Exceptions;
using System.Net.Http.Headers;
using System.Text;
using Application.Interfaces.ExternalServices;
using Application.Interfaces.ExternalServices.AIProviderStrategy;
using Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Infrastructure.ExternalServices
{
    public class ModelGradingImplementation : ILlmGradingService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenRouter _openRouter;
        private readonly IPayloadBuider _payloadBuilder;
        public ModelGradingImplementation(HttpClient httpClient, IPayloadBuider payloadBuider, IOptions<OpenRouter> openRouter)
        {
            _httpClient = httpClient;
            _payloadBuilder = payloadBuider;
            _openRouter = openRouter.Value;

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _openRouter.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
        }
        public async Task<bool> ModelGrading(string studentAnswer, string instructorAnswer, string questionText)
        {
            var prompt = BuildPrompt(questionText, instructorAnswer, studentAnswer);

            if (string.IsNullOrWhiteSpace(prompt))
                throw new ApiException("Prompt cannot be null or empty.", 400, nameof(prompt), null);

            string url = _openRouter.Url;
            var payload = _payloadBuilder.BuildPayload(prompt, "grading");

            if (payload is null)
                throw new ApiException("Payload cannot be null or empty.", 400, nameof(payload), null);

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
                throw new ApiException("AI response failed", (int)response.StatusCode, "FailedRequest", null);

            var responseString = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(responseString);

                var contentText = doc
                    .RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(contentText))
                    throw new ApiException("Empty content from AI", 422, "EMPTY_AI_CONTENT", null);

                // Remove extra quotes and unescape if it's a quoted JSON string
                var cleaned = contentText.Trim();
                if (cleaned.StartsWith("\"") && cleaned.EndsWith("\""))
                {
                    cleaned = JsonSerializer.Deserialize<string>(cleaned);
                }

                
                var result = JsonSerializer.Deserialize<LlmGradingResultDto>(cleaned);

                return result?.IsCorrect ?? false;
            }
            catch (Exception ex)
            {
                throw new ApiException("Failed to parse AI grading response", 500, "GRADING_PARSE_ERROR", ex);
            }
        }

        public static string BuildPrompt(string question, string expectedAnswer, string studentAnswer)
        {
            return $@"
                Evaluate if the student's answer is semantically correct, even if it's not word-for-word. 
                Return a JSON object in this format: {{ isCorrect: true/false, reason: string }}
                Question: {question}
                InstructorAnswer: {expectedAnswer}
                StudentAnswer: {studentAnswer}
                ";
        }
    }
}
