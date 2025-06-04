using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Services.GradingStrategyInterfaces.Interfaces;
using Domain.Entitties;

namespace Application.Services.GradingStrategy.Implementation
{
    public class ObjectiveGradingStrategy : IGradingStrategy
    {
        public Task GradeAsync(AnswerSubmission answerSubmission)
        {
            bool isCorrect = answerSubmission.SubmittedAnswer.Trim().Equals(answerSubmission.Question.Answer.AnswerText.Trim(), StringComparison.OrdinalIgnoreCase);

            answerSubmission.IsCorrect = isCorrect;
            answerSubmission.Score = isCorrect ? answerSubmission.Question.Marks : (short)0;
            return Task.CompletedTask;
        }
    }
}
