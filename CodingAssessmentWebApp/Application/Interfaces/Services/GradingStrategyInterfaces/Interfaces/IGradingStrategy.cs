using Domain.Entitties;
using Domain.Enum;

namespace Application.Interfaces.Services.GradingStrategyInterfaces.Interfaces
{
    public interface IGradingStrategy
    {
        QuestionType QuestionType { get; }
        Task GradeAsync(AnswerSubmission answerSubmission);
    }
}
