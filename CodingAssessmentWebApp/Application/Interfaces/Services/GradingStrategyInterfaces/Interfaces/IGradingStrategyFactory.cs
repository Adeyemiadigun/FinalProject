using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enum;

namespace Application.Interfaces.Services.GradingStrategyInterfaces.Interfaces
{
    public interface IGradingStrategyFactory
    {
        IGradingStrategy GetStrategy(QuestionType questionType);
    }
}
