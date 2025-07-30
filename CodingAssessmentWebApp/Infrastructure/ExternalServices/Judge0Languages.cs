using Application.Exceptions;
using Application.Interfaces.ExternalServices;
using Domain.Enum;

namespace Infrastructure.ExternalServices
{
    public class Judge0Languages : IJudge0Languages
    {
        public readonly Dictionary<TechnologyStack, (string Name, int Id)> Map = new()
            {
                { TechnologyStack.CSharp, ("C# (.NET 6.0)", 51) },
                { TechnologyStack.Java, ("Java (OpenJDK 17.0.6)", 62) },
                { TechnologyStack.Python, ("Python (3.10.0)", 71) },
                { TechnologyStack.JavaScript, ("Node.js (18.15.0)", 63) }
            };

        public int GetLanguageId(TechnologyStack techStack)
        {
            if (Map.TryGetValue(techStack, out var langInfo))
            {
                return langInfo.Id;
            }
            throw new ApiException(
                $"Language {techStack} not found in Judge0 languages.",
                statusCode: 404,
                errorCode: "LanguageNotFound",
                error: null
            );
        }
    }
}
