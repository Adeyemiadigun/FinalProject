using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enum;

namespace Application.Interfaces.ExternalServices.AIProviderStrategy
{
    public interface IAIProviderStrategy
    {
        QuestionType QuestionType { get; }
        Task<string> GenerateTextAsync(string prompt);
    }
}
