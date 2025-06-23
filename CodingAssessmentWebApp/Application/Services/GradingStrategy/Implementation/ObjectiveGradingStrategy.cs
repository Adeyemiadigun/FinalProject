using Application.Interfaces.Services.GradingStrategyInterfaces.Interfaces;
using Domain.Entitties;
using Domain.Enum;

namespace Application.Services.GradingStrategy.Implementation
{ 
    public class ObjectiveGradingStrategy : IGradingStrategy
    {
        public QuestionType QuestionType => QuestionType.Objective;
        public async Task GradeAsync(AnswerSubmission answerSubmission)
        {
            bool isCorrect = answerSubmission.SubmittedAnswer.Trim().Equals(answerSubmission.Question.Answer.AnswerText.Trim(), StringComparison.OrdinalIgnoreCase);

            answerSubmission.IsCorrect = isCorrect;
            answerSubmission.Score = isCorrect ? answerSubmission.Question.Marks : (short)0;
        }
    }
}
