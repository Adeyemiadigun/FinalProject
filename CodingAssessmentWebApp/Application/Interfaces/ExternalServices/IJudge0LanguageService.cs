using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;

namespace Application.Interfaces.ExternalServices
{
    public interface IJudge0LanguageService
    {
        Task<List<Judge0LanguageDto>> GetSupportedLanguagesAsync();
    }
}
