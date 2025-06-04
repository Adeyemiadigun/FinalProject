using Application.Exceptions;
using Application.Interfaces.Services.GradingStrategyInterfaces.Interfaces;
using Domain.Enum;

namespace Application.Services.GradingStrategy.Implementation
{
    public class GradingStrategyFactory(IEnumerable<IGradingStrategy> _strategies) : IGradingStrategyFactory
    {
        public IGradingStrategy GetStrategy(QuestionType questionType)
        {
            return _strategies.FirstOrDefault(s => s.QuestionType == questionType)
                   ?? throw new ApiException($"Grading strategy for question type {questionType} not found.", 404, "GradingStrategyNotFound", null);
        }
    }
}


