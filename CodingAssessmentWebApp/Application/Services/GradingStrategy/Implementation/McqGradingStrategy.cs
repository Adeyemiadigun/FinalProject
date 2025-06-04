using Application.Interfaces.Services.GradingStrategyInterfaces.Interfaces;
using Domain.Entitties;
using Domain.Enum;

namespace Application.Services.GradingStrategy.Implementation
{
    public class McqGradingStrategy : IGradingStrategy
    {
        public QuestionType QuestionType =>  QuestionType.MCQ;
        public Task GradeAsync(AnswerSubmission answerSubmission)
        {
            var correctOptionIds = answerSubmission.Question.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToList();
            var selectedOptionIds = answerSubmission.SelectedOptionIds ?? [];

            bool isCorrect = correctOptionIds.Count == selectedOptionIds.Count &&
                             correctOptionIds.All(selectedOptionIds.Contains);

            answerSubmission.IsCorrect = isCorrect;
            answerSubmission.Score = isCorrect ? answerSubmission.Question.Marks : (short)0;
            return Task.CompletedTask;
        }
    }

}
