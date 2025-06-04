using Application.Exceptions;
using Application.Interfaces.Services.GradingStrategyInterfaces.Interfaces;
using Domain.Enum;

namespace Application.Services.GradingStrategy.Implementation
{
    public class GradingStrategyFactory : IGradingStrategyFactory
    {
        public IGradingStrategy GetStrategy(QuestionType questionType)
        {
            return questionType switch
            {
                QuestionType.MCQ => new McqGradingStrategy(),
                QuestionType.Objective => new ObjectiveGradingStrategy(),
                //QuestionType.Coding => new CodingGradingStrategy(),
                _ => throw new ApiException("Unknown question type", 400, "InvalidQuestionType", null)
            };
        }
    }
}


