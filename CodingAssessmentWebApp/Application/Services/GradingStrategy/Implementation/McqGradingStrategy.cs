using Application.Interfaces.Services.GradingStrategyInterfaces.Interfaces;
using Domain.Entitties;
using Domain.Enum;

namespace Application.Services.GradingStrategy.Implementation
{
    public class McqGradingStrategy : IGradingStrategy
    {
        public QuestionType QuestionType =>  QuestionType.MCQ;
        public async Task GradeAsync(AnswerSubmission answerSubmission)
        {
            var correctOptionIds = answerSubmission.Question.Options
                   .Where(o => o.IsCorrect)
                   .Select(o => o.Id)
                   .OrderBy(x => x)
                   .ToList();

            var selectedOptionIds = answerSubmission.SelectedOptions
                .Select(x => x.OptionId)
                .OrderBy(x => x)
                .ToList();

            bool isCorrect = correctOptionIds.SequenceEqual(selectedOptionIds);

            answerSubmission.IsCorrect = isCorrect;
            answerSubmission.Score = isCorrect ? answerSubmission.Question.Marks : (short)0;
        }
    }

}
