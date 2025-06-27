using Application.Exceptions;
using Application.Interfaces.ExternalServices.AIProviderStrategy;
using Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Application.Services
{
    public class PayloadBuilder : IPayloadBuider
    {
        private readonly PayloadTemplateSettings _payloadTemplate;
        public PayloadBuilder(IOptions<PayloadTemplateSettings> payloadBuider)
        {
            _payloadTemplate = payloadBuider.Value;
        }
        public string BuildPayload(string name, string prompt)
        {
            var template = _payloadTemplate.PayloadTemplateSetting[name];
            if (string.IsNullOrWhiteSpace(template))
            {
                return null!;
            }
            var actualPayload = template.Replace("{{prompt}}", prompt);
            return actualPayload;
        }
    }
}
