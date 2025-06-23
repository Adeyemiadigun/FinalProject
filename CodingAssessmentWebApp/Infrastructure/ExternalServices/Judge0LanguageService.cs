using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces.ExternalServices;

namespace Infrastructure.ExternalServices
{
    public class Judge0LanguageService : IJudge0LanguageService
    {
        private readonly HttpClient _httpClient;
        public Judge0LanguageService() 
        {
            _httpClient = new HttpClient();
        }
        public async Task<List<Judge0LanguageDto>> GetSupportedLanguagesAsync()
        {
            var response = await _httpClient.GetAsync("https://ce.judge0.com/languages");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to fetch supported languages from Judge0.");
            }
            var content = await response.Content.ReadAsStringAsync();
            var judge0Languages = JsonSerializer.Deserialize<List<Judge0LanguageDto>>(content);
            return judge0Languages;
        }
    }
}
