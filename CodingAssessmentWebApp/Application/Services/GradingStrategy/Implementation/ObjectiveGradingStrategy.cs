using Application.Interfaces.ExternalServices;
using Application.Interfaces.Services.GradingStrategyInterfaces.Interfaces;
using Domain.Entitties;
using Domain.Enum;

namespace Application.Services.GradingStrategy.Implementation
{ 
    public class ObjectiveGradingStrategy(ILlmGradingService llmGradingService) : IGradingStrategy
    {
        public QuestionType QuestionType => QuestionType.Objective;
        public async Task GradeAsync(AnswerSubmission answerSubmission)
        {
           var result = await llmGradingService.ModelGrading(answerSubmission.SubmittedAnswer, answerSubmission.Question.Answer.AnswerText.Trim(), answerSubmission.Question.QuestionText);

            bool isCorrect = result;

            answerSubmission.IsCorrect = isCorrect;
            answerSubmission.Score = isCorrect ? answerSubmission.Question.Marks : (short)0;
        }
    }
}
