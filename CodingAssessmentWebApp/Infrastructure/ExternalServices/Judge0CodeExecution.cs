using Application.Dtos;
using Application.Interfaces.ExternalServices;
using Domain.Entitties;

namespace Infrastructure.ExternalServices
{
    public class Judge0CodeExecution : ICodeExcution
    {
        private readonly HttpClient _httpClient;
        public Judge0CodeExecution() 
        {
            _httpClient = new HttpClient();
        }

        public async Task<Judge0Result> ExecuteCodeAsync(Judge0CodeExecutionRequest answer)
        {

            var payload = new
            {
                Language_id = answer.Id,
                Source_Code = answer.SourceCode,
                Stdin = answer.TestCase,
                Expected_output = answer.ExpectedOutput,
                Cpu_time_limit = 2 // seconds
            };
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://ce.judge0.com/submissions?base64_encoded=false&wait=true", content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to execute code");
            }
            var result = await response.Content.ReadAsStringAsync();
            var judge0Result = System.Text.Json.JsonSerializer.Deserialize<Judge0Result>(result);
            return judge0Result ?? throw new Exception("Failed to deserialize Judge0 result");
        }
  
    }
}
