using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces.ExternalServices;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.ExternalServices
{
    public class Judge0LanguageService : IJudge0LanguageService
    {
        private readonly HttpClient _httpClient;
        private readonly string Key;
        private readonly IConfiguration configuration;
        public Judge0LanguageService(IConfiguration config) 
        {
            configuration = config;
            Key = configuration["Judge0APiKey:key"]!;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", "judge0-ce.p.rapidapi.com");
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", Key);
        }
        public async Task<List<Judge0LanguageDto>> GetSupportedLanguagesAsync()
        {
            var response = await _httpClient.GetAsync("https://judge0-ce.p.rapidapi.com/languages ");
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

