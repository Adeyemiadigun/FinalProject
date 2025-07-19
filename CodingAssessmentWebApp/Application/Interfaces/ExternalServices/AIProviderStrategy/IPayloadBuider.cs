namespace Application.Interfaces.ExternalServices.AIProviderStrategy
{
    public interface IPayloadBuider
    {
        string BuildPayload(string prompt, string payloadType);
    }
}
