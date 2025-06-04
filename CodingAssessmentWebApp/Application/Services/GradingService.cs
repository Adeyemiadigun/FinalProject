using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.GradingStrategyInterfaces.Interfaces;
using Domain.Entitties;

namespace Application.Services
{
    public class GradingService(IGradingStrategyFactory gradingStrategyFactory) : IGradingService
    {
        public async Task GradeSubmissionAsync(Submission submission)
        {
            var totalScore = 0;
            foreach (var answer in submission.AnswerSubmissions)
            {
                var gradingStrategy = gradingStrategyFactory.GetStrategy(answer.Question.QuestionType);
                if (gradingStrategy == null)
                {
                    throw new ApiException(
                        $"No grading strategy found for question type: {answer.Question.QuestionType}",
                        statusCode: 400,
                        errorCode: "GRADING_STRATEGY_NOT_FOUND",
                        error: null
                    );
                }
                await gradingStrategy.GradeAsync(answer);
                totalScore += answer.Score;
            }
            submission.TotalScore = (short)totalScore;
        }
    }
}
