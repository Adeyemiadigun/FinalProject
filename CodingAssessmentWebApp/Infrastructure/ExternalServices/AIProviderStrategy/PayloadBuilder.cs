using System.Text.Json;
using Application.Exceptions;
using Application.Interfaces.ExternalServices.AIProviderStrategy;
using Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Application.Services
{
    using System.Text.Json;

    public class PayloadBuilder : IPayloadBuider
    {
        public string BuildPayload(string prompt, string payloadType)
        {
            string template = null;
            if(payloadType == "questionGen")
             template = Payload(); 
            if(payloadType == "grading")
                template = GradingPayload();
            // Just the payload format string
            if (string.IsNullOrWhiteSpace(template))
                return null!;

            // Escape the prompt safely for JSON context
            var escapedPrompt = JsonSerializer.Serialize(prompt).Trim('"');
            var actualPayload = template.Replace("{{prompt}}", escapedPrompt);
            return actualPayload;
        }
       

        private string Payload()
        {
            // Only return the Mistral payload template string
            return "{ \\\"model\\\": \\\"mistralai/mistral-7b-instruct:free\\\", \\\"messages\\\": [ { \\\"role\\\": \\\"system\\\", \\\"content\\\": \\\"You are a question generator that returns output in clean JSON format only.\\\" }, { \\\"role\\\": \\\"user\\\", \\\"content\\\": \\\"{{prompt}}\\\" } ] }";
        }
        public string GradingPayload()
        {
            return "{ \\\"model\\\": \\\"mistralai/mistral-7b-instruct:free\\\", \\\"messages\\\": [ { \\\"role\\\": \\\"system\\\", \\\"content\\\": \\\"Evaluate if the student's answer is semantically correct, You are grading a short objective question. even if it's not word-for-word. Return { isCorrect: true/false, reason: string }\\\" }, { \\\"role\\\": \\\"user\\\", \\\"content\\\": \\\"{{prompt}}\\\" } ] }";
        }


    }

}
