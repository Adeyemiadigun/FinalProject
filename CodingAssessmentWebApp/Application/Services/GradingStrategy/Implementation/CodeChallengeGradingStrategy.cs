using System.Text.RegularExpressions;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.ExternalServices;
using Application.Interfaces.Services.GradingStrategyInterfaces.Interfaces;
using Domain.Entities;
using Domain.Entitties;
using Domain.Enum;

namespace Application.Services.GradingStrategy.Implementation
{
    public class CodeChallengeGradingStrategy(IJudge0LanguageService _judge, IJudge0LanguageStore _store, ICodeExcution _excute,ICodeWrapper codeWrapper, IExtractMethodName extractMethodName) : IGradingStrategy

    {
        public QuestionType QuestionType => QuestionType.Coding;

        public async Task GradeAsync(AnswerSubmission answerSubmission)
        {
            Judge0LanguageDto stack = null;
            var techStack = (TechnologyStack)Enum.Parse(typeof(TechnologyStack), answerSubmission.Question.TechnologyStack.ToString());
            if(techStack == TechnologyStack.CSharp)
            {
               stack  = await _store.GetLanguageByName("C#");
            }

            stack = await _store.GetLanguageByName(techStack.ToString());

            if (stack is null)
            {
                var languages = await _judge.GetSupportedLanguagesAsync();
                await _store.SaveLanguages(languages);
                stack = await _store.GetLanguageByName(techStack.ToString());
                if (stack is null)
                    throw new ApiException("Code execution failed", 500, "ExecutionError", null);
            }

            string studentCode = answerSubmission.SubmittedAnswer;
            string methodName = extractMethodName.ExtractMethodName(studentCode,answerSubmission.Question.TechnologyStack.Value) ?? "Solve";

            int totalWeight = 0;
            
            foreach (var test in answerSubmission.Question.Tests)
            {

                string wrappedCode = codeWrapper.Wrap(techStack, studentCode, methodName, test.Input);
                var result = await _excute.ExecuteCodeAsync(new Judge0CodeExecutionRequest
                {
                    SourceCode = wrappedCode,
                    Id = stack.Id,
                    ExpectedOutput = test.ExpectedOutput
                }) ?? throw new ApiException("Code execution failed", 500, "ExecutionError", null);

                bool isCorrect = result.StdOut?.Trim() == test.ExpectedOutput.Trim();

                answerSubmission.TestCaseResults.Add(new TestCaseResult
                {
                    AnswerSubmissionId = answerSubmission.Id,
                    Input = test.Input,
                    ExpectedOutput = test.ExpectedOutput,
                    ActualOutput = result.StdOut,
                    Passed = isCorrect,
                    EarnedWeight = isCorrect ? test.Weight : 0
                });

                totalWeight += isCorrect ? test.Weight : 0;
            }

            answerSubmission.Score = (short)totalWeight;
            answerSubmission.IsCorrect = totalWeight == answerSubmission.Question.Tests.Sum(t => t.Weight);
        }

    }
}
