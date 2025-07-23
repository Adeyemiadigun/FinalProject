using System.Collections.Concurrent;
using Application.Dtos;
using Application.Interfaces.ExternalServices;

namespace Infrastructure.ExternalServices
{
    public class Judge0LanguageStore : IJudge0LanguageStore
    {
        private readonly ConcurrentDictionary<string, Judge0LanguageDto> _store = new();
        public async Task<Judge0LanguageDto?> GetLanguageByName(string languageName)
        {
            var match = _store
                .FirstOrDefault(x => x.Key.Contains(languageName, StringComparison.OrdinalIgnoreCase));

            return match.Equals(default(KeyValuePair<string, Judge0LanguageDto>))
                ? null
                : match.Value;
        }

        public async Task SaveLanguages(List<Judge0LanguageDto> languages)
        {
            foreach (var item in languages)
            {
                 _store[item.Name] = item;
            }
        }
    }
}
