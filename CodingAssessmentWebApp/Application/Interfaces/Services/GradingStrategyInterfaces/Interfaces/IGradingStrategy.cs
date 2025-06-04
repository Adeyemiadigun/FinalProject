using Domain.Entitties;

namespace Application.Interfaces.Services.GradingStrategyInterfaces.Interfaces
{
    public interface IGradingStrategy
    {
        Task GradeAsync(AnswerSubmission answerSubmission);
    }
}
