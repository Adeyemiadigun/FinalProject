using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.ExternalServices.AIProviderStrategy
{
    public interface IAIProviderStrategy
    {
        Task<string> GenerateTextAsync(string prompt);
    }

}
