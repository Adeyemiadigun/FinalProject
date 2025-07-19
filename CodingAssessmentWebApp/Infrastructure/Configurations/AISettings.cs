using Domain.Enum;

namespace Infrastructure.Configurations
{
    public class AISettings
    {
        public Dictionary<string, List<AIProviderConfig>> QuestionTypeProviders { get; set; } = new();
    }
    public class AIProviderConfig
    {
        public string Name { get; set; }
        public string Model { get; set; }
        public string ApiKey { get; set; }
        public string ApiUrl { get; set; }
        public bool Enabled { get; set; }
        public string AuthType { get; set; }

    }
    public class OpenRouter
    {
        public string ApiKey { get; set; }
        public string Url { get; set; }
        public string HttpReferer { get; set; }
    }
}
