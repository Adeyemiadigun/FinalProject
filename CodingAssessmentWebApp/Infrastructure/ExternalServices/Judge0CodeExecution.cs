using Application.Dtos;
using Application.Interfaces.ExternalServices;
using Domain.Entitties;
using Microsoft.Extensions.Configuration;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Infrastructure.ExternalServices
{
    public class Judge0CodeExecution : ICodeExcution
    {
        private readonly HttpClient _httpClient;
        private readonly string Key;
        private readonly IConfiguration configuration;
        public Judge0CodeExecution(IConfiguration config)
        {
            _httpClient = new HttpClient();
            configuration = config;
            Key = configuration["Judge0APiKey:key"]!;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", "judge0-ce.p.rapidapi.com");
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", Key);
        }

        public async Task<Judge0Result> ExecuteCodeAsync(Judge0CodeExecutionRequest answer)
        {
            var payload = new
            {
                language_id = answer.Id,
                source_code = answer.SourceCode,
                stdin = answer.TestCase,
                expected_output = answer.ExpectedOutput,
                cpu_time_limit = 2
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                "https://judge0-ce.p.rapidapi.com/submissions?base64_encoded=false&wait=true",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                throw new Exception($"Judge0 request failed: {response.StatusCode}\n{errorDetails}");
            }

            var resultJson = await response.Content.ReadAsStringAsync();
            var judge0Result = System.Text.Json.JsonSerializer.Deserialize<Judge0Result>(resultJson);

            return judge0Result ?? throw new Exception("Failed to deserialize Judge0 result");
        }
    }

}
