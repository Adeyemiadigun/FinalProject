using System.Linq.Expressions;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entitties;
using Domain.Enum;

namespace Application.Services
{
    public class QuestionService(IQuestionRepository _questionRepository, IUnitOfWork _unitOfWork) : IQuestionService
    {
        public async Task<bool> CreateQuestionsAsync(List<CreateQuestionRequestModel> model, Guid assessmentId)
        {
            try
            {
                var questions = model.Select(x => new Question
                {
                    QuestionText = x.QuestionText,
                    QuestionType = x.QuestionType,
                    AssessmentId = assessmentId,
                    Options = x.QuestionType == QuestionType.MCQ
                        ? x.Options.Select(o => new Option
                        {
                            OptionText = o.OptionText,
                            IsCorrect = o.IsCorrect
                        }).ToList()
                        : null!,
                    Tests = x.QuestionType == QuestionType.Coding
                        ? x.TestCases.Select(t => new TestCase
                        {
                            Input = t.Input,
                            ExpectedOutput = t.ExpectedOutput,
                            Weight = (short)t.Weight
                        }).ToList()
                        : null!,
                    Answer = x.QuestionType == QuestionType.Objective ? new Answer()
                    {
                        QuestionId = x.Answer.QuestionId,
                        Question = x.Answer.Question,
                        AnswerText = x.Answer.AnswerText
                    } : null!,
                    Marks = (short)x.Marks,
                    Order = x.Order,

                }).ToList();
                await _questionRepository.CreateAsync(questions);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new ApiException("An error occurred while creating questions.", 500, "CREATE_QUESTIONS_ERROR", ex);
            }
        }
    }
}
