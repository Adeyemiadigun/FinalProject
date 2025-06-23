using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;

namespace Application.Interfaces.ExternalServices
{
    public interface IJudge0LanguageStore
    {
        Task SaveLanguages(List<Judge0LanguageDto> languages);
        Task<Judge0LanguageDto> GetLanguageByName(string languageName);
    }
}
