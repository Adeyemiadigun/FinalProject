using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.ExternalServices;
using Application.Interfaces.Services.GradingStrategyInterfaces.Interfaces;
using Domain.Entities;
using Domain.Entitties;
using Domain.Enum;
using static System.Net.Mime.MediaTypeNames;

namespace Application.Services.GradingStrategy.Implementation
{
    public class CodeChallengeGradingStrategy(IJudge0LanguageService _judge, IJudge0LanguageStore _store, ICodeExcution _excute) : IGradingStrategy
    {
        public QuestionType QuestionType => QuestionType.Coding;

        public async Task GradeAsync(AnswerSubmission answerSubmission)
        {
            var stack = await _store.GetLanguageByName(answerSubmission.Question.TechnologyStack.ToString()!);

            if (stack is null)
            {
               var languages = await _judge.GetSupportedLanguagesAsync();
               await _store.SaveLanguages(languages);
                stack = await _store.GetLanguageByName(answerSubmission.Question.TechnologyStack.ToString()!);
                if(stack is null)
                    throw new ApiException()
            }
            var totalWeigtht = 0;
            foreach (var test in answerSubmission.Question.Tests)
            {
                var result = await _excute.ExecuteCodeAsync(new Judge0CodeExecutionRequest
                {
                    SourceCode = answerSubmission.SubmittedAnswer,
                    Id = stack.Id,
                    TestCase = test.Input,
                    ExpectedOutput = test.ExpectedOutput,
                }) ?? throw new ApiException("Code execution failed", 500, "ExecutionError", null);

                bool isCorrect = result.StdOut.Trim() == test.ExpectedOutput.Trim();
                var testCaseResult = new TestCaseResult
                {
                    AnswerSubmissionId = answerSubmission.Id,
                    Input = test.Input,
                    ExpectedOutput = test.ExpectedOutput,
                    ActualOutput = result.StdOut,
                    Passed = result.StdOut == test.ExpectedOutput,
                    EarnedWeight = isCorrect ? test.Weight : 0
                };
                answerSubmission.TestCaseResults.Add(testCaseResult);
                totalWeigtht += isCorrect ? test.Weight : 0;
            }
            answerSubmission.Score = (short)totalWeigtht;
            answerSubmission.IsCorrect = totalWeigtht == answerSubmission.Question.Tests.Sum(t => t.Weight);

        }
    }
}
