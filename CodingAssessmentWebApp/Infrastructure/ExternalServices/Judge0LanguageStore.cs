using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces.ExternalServices;
using Domain.ValueObjects;

namespace Infrastructure.ExternalServices
{
    public class Judge0LanguageStore : IJudge0LanguageStore
    {
        private readonly ConcurrentDictionary<string, Judge0LanguageDto> _store = new();
        public async Task<Judge0LanguageDto> GetLanguageByName(string languageName)
        {
            var language =  _store.Keys.Contains(languageName) ? _store[languageName] : null;
            return language;
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
