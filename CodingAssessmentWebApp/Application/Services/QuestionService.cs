using System.Linq.Expressions;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entitties;
using Domain.Enum;

namespace Application.Services
{
    public class QuestionService(IQuestionRepository _questionRepository, IUnitOfWork _unitOfWork, IAssessmentRepository _assessmentRepository, ICurrentUser _currentUser) : IQuestionService
    {
        public async Task<bool> CreateQuestionsAsync(List<CreateQuestionRequestModel> model, Guid assessmentId)
        {
            try
            {
                var currentUser = _currentUser.GetCurrentUserId();
                if (currentUser == Guid.Empty)
                    throw new ApiException("User not authenticated.", 401, "USER_NOT_AUTHENTICATED", null);

                if (model == null || !model.Any())
                    throw new ApiException("No questions provided.", 400, "NO_QUESTIONS_PROVIDED", null);

                var assessment = await _assessmentRepository.GetAsync(assessmentId) ??
                    throw new ApiException("Assessment not found.", 404, "ASSESSMENT_NOT_FOUND", null);
                    if(currentUser != assessment.InstructorId)
                    throw new ApiException("You are not authorized to add questions to this assessment.", 403, "UNAUTHORIZED_ACCESS", null);
                TechnologyStack techStack;
                if(assessment.TechnologyStack == "c#" || assessment.TechnologyStack =="C#")
                    techStack = TechnologyStack.CSharp;
                else
                    techStack = (TechnologyStack)Enum.Parse(typeof(TechnologyStack), assessment.TechnologyStack);
              
                var questions = model.Select(x => new Question
                {
                    QuestionText = x.QuestionText,
                    QuestionType = x.QuestionType,
                    AssessmentId = assessmentId,
                    Assessment = assessment,
                    TechnologyStack = techStack,
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

        public async Task<BaseResponse<QuestionDto>> DeleteQuestionAsync(Guid id)
        {
            try
            {
                var question = await _questionRepository.GetAsync(id) ??
                    throw new ApiException("Question not found.", 404, "QUESTION_NOT_FOUND", null);

                var currentUser = _currentUser.GetCurrentUserId();
                var assessment = await _assessmentRepository.GetAsync(question.AssessmentId) ??
                    throw new ApiException("Assessment not found.", 404, "ASSESSMENT_NOT_FOUND", null);

                if (assessment.InstructorId != currentUser)
                    throw new ApiException("You are not authorized to delete this question.", 403, "UNAUTHORIZED_ACCESS", null);

                await _questionRepository.Delete(question);
      
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse<QuestionDto>
                {
                    Message = "Question deleted successfully.",
                    Status = true,
                    Data = null
                };
            }
            catch (Exception ex)
            {
                throw new ApiException("An error occurred while deleting the question.", 500, "DELETE_QUESTION_ERROR", ex);
            }
        }

        public async Task<BaseResponse<List<QuestionDto>>> GetAllQuestionsByAssessmentIdAsync(Guid assessmentId)
        {
            var check = await _assessmentRepository.CheckAsync(x => x.Id == assessmentId);
            if (!check)
                throw new ApiException("Assessment not found.", 404, "ASSESSMENT_NOT_FOUND", null);

            var questions = await _questionRepository.GetAllAsync(assessmentId);
            if (questions == null || !questions.Any())
                throw new ApiException("No questions found for the given assessment.", 404, "NO_QUESTIONS_FOUND", null);

            var questionDtos = questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Marks = q.Marks,
                Order = q.Order,
                Answer = q.QuestionType == QuestionType.Objective ? new CreateAnswerDto
                {
                    AnswerText = q.Answer.AnswerText,
                } : null!,
                Options = q.QuestionType == QuestionType.MCQ ? q.Options?.Select(o => new OptionDto
                {
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect
                }).ToList() : null!,
                TestCases = q.QuestionType==QuestionType.Coding ? q.Tests?.Select(t => new CreateTestCaseDto
                {
                    Input = t.Input,
                    ExpectedOutput = t.ExpectedOutput,
                    Weight = t.Weight
                }).ToList() : null!
            }).OrderByDescending(x => x.Order).ToList();
            return new BaseResponse<List<QuestionDto>>
            {
                Message = "Questions retrieved successfully.",
                Status = true,
                Data = questionDtos
            };
        }

        public Task<BaseResponse<QuestionDto>> GetQuestionAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<QuestionDto>> UpdateQuestion(Guid id, UpdateQuestionDto model)
        {
            var question = await _questionRepository.GetAsync(id) ??
                throw new ApiException("Question not found.", 404, "QUESTION_NOT_FOUND", null);
            var currentUser = _currentUser.GetCurrentUserId();

            var assessment = await _assessmentRepository.GetAsync(question.AssessmentId) ??
                throw new ApiException("Assessment not found.", 404, "ASSESSMENT_NOT_FOUND", null);

            if (assessment.InstructorId != currentUser)
                throw new ApiException("You are not authorized to update questions for this assessment.", 403, "UNAUTHORIZED_ACCESS", null);

            question.QuestionText = model.QuestionText;
            question.Marks = (short)model.Marks;
            question.QuestionType = model.QuestionType;
            question.Answer = QuestionType.Objective == model.QuestionType ? new Answer
            {
                QuestionId = question.Id,
                Question = question,
                AnswerText = model.Answer.AnswerText
            } : null!;
            question.Options = QuestionType.MCQ == model.QuestionType ? model.Options.Select(x =>
            new Option
            {
                QuestionId = question.Id,
                Question = question,
                OptionText = x.OptionText,
                IsCorrect = x.IsCorrect,
            }).ToList() : null!;

            question.Tests = QuestionType.Coding == model.QuestionType ? model.TestCases.Select(x => new TestCase
            {
                QuestionId = question.Id,
                Question = question,
                ExpectedOutput = x.ExpectedOutput,
                Input = x.Input,
                Weight = (short)x.Weight,
            }).ToList() : null!;

            _questionRepository.Update(question);
            await _unitOfWork.SaveChangesAsync();

            var questionDto = new QuestionDto
            {
                Id = question.Id,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType,
                Marks = question.Marks,
                Order = question.Order,
                Answer = new CreateAnswerDto
                {
                    AnswerText = question.QuestionText,
                } ?? null!,

                Options = question.Options?.Select(o => new OptionDto
                {
                    OptionText = o.OptionText,
                }).ToList() ?? null!,
                TestCases = question.Tests?.Select(t => new CreateTestCaseDto
                {
                    Input = t.Input,
                    ExpectedOutput = t.ExpectedOutput,
                    Weight = t.Weight
                }).ToList() ?? null!
            };

            return new BaseResponse<QuestionDto>
            {
                Message = "Question updated successfully.",
                Status = true,
                Data = questionDto
            };
        }
        public async Task<BaseResponse<StudentQuestionAssessmentDto>> GetAssessmentForAttemptAsync(Guid assessmentId)   
        {
            var studentId = _currentUser.GetCurrentUserId();

            var assessment = await _assessmentRepository.GetAsync(assessmentId);
            if (assessment is null)
                throw new ApiException("Assessment not found.", 404, "ASSESSMENT_NOT_FOUND", null);
            var currentDate = DateTime.UtcNow;
            if (assessment.StartDate > currentDate)
                throw new ApiException("Assessment has not started yet.", 404, "ASSESMENT_NOT_STARTED", null);
            if (assessment.EndDate < currentDate)
                throw new ApiException("Assessment has Ended yet.", 404, "ASSESMENT_NOT_STARTED", null);
            var questions = await _questionRepository.GetAllAsync(assessmentId);
            if (questions == null || !questions.Any())
                throw new ApiException("No questions found for the given assessment.", 404, "NO_QUESTIONS_FOUND", null);
            var questionDtos = questions.Select(q => new StudentQuestionDto
            {
                Id = q.Id,
                Title = q.QuestionText,
                Type = q.QuestionType.ToString(),
                TechStack = q.TechnologyStack.ToString(),
                Options = q.Options?.Select(o => new StudentOptionDto
                {
                    Id = o.Id,
                    Text = o.OptionText
                }).ToList(),    
                TestCases = q.Tests?.Select(t => new StudentTestCaseDto
                {
                    Input = t.Input,
                    Weight = t.Weight
                }).ToList()
            }).ToList();

            return new BaseResponse<StudentQuestionAssessmentDto>
            {
                Status = true,
                Message = "Assessment for student loaded",
                Data = new StudentQuestionAssessmentDto
                {
                    Id = assessment.Id,
                    Title = assessment.Title,
                    DurationInMinutes = assessment.DurationInMinutes,
                    Questions = questionDtos
                }
            };
        }

    }
}
