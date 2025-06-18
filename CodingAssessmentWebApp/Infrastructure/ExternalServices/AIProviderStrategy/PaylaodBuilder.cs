using Application.Exceptions;
using Application.Interfaces.ExternalServices.AIProviderStrategy;
using Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Application.Services
{
    public class PaylaodBuilder : IPayloadBuider
    {
        private readonly PayloadTemplateSettings _payloadTemplate;
        public PaylaodBuilder(IOptions<PayloadTemplateSettings> payloadBuider)
        {
            _payloadTemplate = payloadBuider.Value;
        }
        public string BuildPayload(string name, string prompt)
        {
            var template = _payloadTemplate.PayloadTemplates[name];
            if (string.IsNullOrWhiteSpace(template))
            {
                return null!;
            }
            var actualPayload = template.Replace("{{prompt}}", prompt);
            return actualPayload;
        }
    }
}
