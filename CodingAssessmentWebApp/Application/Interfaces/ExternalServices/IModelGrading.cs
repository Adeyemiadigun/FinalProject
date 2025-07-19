namespace Application.Interfaces.ExternalServices
{
    public interface ILlmGradingService
    {
        Task<bool> ModelGrading(string studentAnswer, string InstructorAnswer,string QuestionText);
    }
}
