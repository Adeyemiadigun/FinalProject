using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.ExternalServices;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Entitties;
using Domain.Enum;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Application.Services
{
    public class SubmissionService(IAssessmentRepository _assessmentRepository, ICurrentUser _currentUser, ISubmissionRepository _submissionRepository, IUnitOfWork _unitOfWork, IQuestionRepository _questionRepository, IGradingService _gradingService, IEmailService _emailService, IUserRepository _userRepository, IBackgroundService _backgroundService) : ISubmissionService
    {
        public async Task<BaseResponse<SubmissionDto>> SubmitAssessment(Guid assessmentId, AnswerSubmissionDto submission)
        {
            var studentId = _currentUser.GetCurrentUserId();
            if (studentId == Guid.Empty)
                throw new ApiException("Current user ID is not set or invalid.", 400, "InvalidUserId", null);

            var student = await _userRepository.GetAsync(studentId)
                ?? throw new ApiException("Student not found", 404, "StudentNotFound", null);

            var assessment = await _assessmentRepository.GetForSubmissionAsync(assessmentId)
                ?? throw new ApiException("Assessment not found", 404, "AssessmentNotFound", null);

            if (assessment.Submissions.Any(s => s.StudentId == studentId))
                throw new ApiException("Duplicate submission detected.", 400, "DuplicateSubmission", null);

            if (DateTime.UtcNow > assessment.EndDate)
                return new BaseResponse<SubmissionDto> { Message = "Assessment is closed", Status = false };

            if (DateTime.UtcNow < assessment.StartDate)
                return new BaseResponse<SubmissionDto> { Message = "Assessment has not started yet", Status = false };

            // Check for duplicated answers
            if (submission.Answers.Select(a => a.QuestionId).Distinct().Count() != submission.Answers.Count)
                throw new ApiException("Duplicate answers for the same question are not allowed.", 400, "DUPLICATE_QUESTION_ANSWERS", null);

            var questionIds = submission.Answers.Select(x => x.QuestionId).ToList();
            var questions = await _questionRepository.GetSelectedIds(questionIds);

            if (questionIds.Count != questions.Count)
                return new BaseResponse<SubmissionDto> { Status = false, Message = "Some Question IDs are invalid." };

            var questionDict = questions.ToDictionary(q => q.Id);
            var submissionEntity = new Submission
            {
                StudentId = studentId,
                Student = student,
                AssessmentId = assessmentId,
                SubmittedAt = DateTime.UtcNow,
            };

            var answerSubmissions = new List<AnswerSubmission>();

            foreach (var submittedAnswer in submission.Answers)
            {
                if (!questionDict.TryGetValue(submittedAnswer.QuestionId, out var question))
                    throw new ApiException("Invalid question ID submitted.", 400, "QUESTION_NOT_FOUND", null);

                // Validate input by question type
                switch (question.QuestionType)
                {
                    case QuestionType.MCQ:
                        var selectedOptions = question.Options
                            .Where(o => submittedAnswer.SelectedOptionIds.Contains(o.Id))
                            .ToList();

                        if (selectedOptions.Count != submittedAnswer.SelectedOptionIds.Count)
                        {
                            var invalidIds = submittedAnswer.SelectedOptionIds
                                .Where(id => !question.Options.Any(o => o.Id == id))
                                .ToList();

                            throw new ApiException($"Some selected options are invalid for question '{question.QuestionText}'",
                                400, "INVALID_MCQ_OPTIONS", new { InvalidOptionIds = invalidIds });
                        }

                        answerSubmissions.Add(new AnswerSubmission
                        {
                            Submission = submissionEntity,
                            SubmissionId = submissionEntity.Id,
                            Question = question,
                            QuestionId = question.Id,
                            SubmittedAnswer = submittedAnswer.SubmittedAnswer,
                            SelectedOptions = selectedOptions.Select(o => new SelectedOption
                            {
                                OptionId = o.Id,
                                Option  = o,
                            }).ToList(),
                            Score = 0,
                            IsCorrect = false
                        });
                        break;

                    case QuestionType.Objective:
                    case QuestionType.Coding:
                        if (string.IsNullOrWhiteSpace(submittedAnswer.SubmittedAnswer))
                            throw new ApiException($"Answer required for question '{question.QuestionText}'", 400, "MISSING_TEXT_ANSWER", null);

                        answerSubmissions.Add(new AnswerSubmission
                        {
                            Submission = submissionEntity,
                            SubmissionId = submissionEntity.Id,
                            Question = question,
                            QuestionId = question.Id,
                            SubmittedAnswer = submittedAnswer.SubmittedAnswer,
                            Score = 0,
                            IsCorrect = false
                        });
                        break;

                    default:
                        throw new ApiException("Unsupported question type.", 400, "INVALID_QUESTION_TYPE", null);
                }
            }

            submissionEntity.AnswerSubmissions = answerSubmissions;

            await _submissionRepository.CreateAsync(submissionEntity);
            await _unitOfWork.SaveChangesAsync();

            _backgroundService.Enqueue<IGradingService>(g =>
                g.GradeSubmissionAndNotifyAsync(submissionEntity.Id, studentId));

            return new BaseResponse<SubmissionDto>
            {
                Status = true,
                Message = "Submission created successfully"
            };
        }



        public Task<PaginationDto<Submission>> GetAllAsync(Guid assessmentId, PaginationRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Submission> GetAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<PaginationDto<Submission>> GetStudentSubmissionsAsync(Guid studentId, PaginationRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Submission> GetWithAnswersAsync(Guid id)
        {
            throw new NotImplementedException();
        }
        public Task<BaseResponse<SubmissionDto>> GetCurrentStudentSubmission(Guid assessmentId)
        {
            var userId = _currentUser.GetCurrentUserId();
            if(Guid.Empty == userId)
            {
                throw new ApiException("Current user ID is not set or invalid.", 400, "InvalidUserId", null);
            }
            return GetStudentSubmissionAsync(assessmentId, userId);
        }
       public async Task<BaseResponse<SubmissionDto>> GetStudentSubmissionAsync(Guid assessmentId, Guid studentId)
        {
            var submission = await _submissionRepository.GetAsync(s => s.AssessmentId == assessmentId && s.StudentId == studentId);

            if (submission == null)
            {
                return new BaseResponse<SubmissionDto>
                {
                    Status = false,
                    Message = "Submission not found"
                };
            }

            var submissionDto = new SubmissionDto
            {
                Id = submission.Id,
                AssessmentId = submission.AssessmentId,
                Title = submission.Assessment.Title,
                SubmittedAt = submission.SubmittedAt,
                TotalScore = submission.TotalScore,
                FeedBack = submission.FeedBack,
                SubmittedAnswers = submission.AnswerSubmissions.Select(a => new SubmittedAnswerDto
                {
                    QuestionId = a.QuestionId,
                    QuestionText = a.Question.QuestionText,
                    QuestionType = a.Question.QuestionType,
                    SubmittedAnswer = a.SubmittedAnswer,
                    Order = a.Question.Order,
                    IsCorrect = a.IsCorrect,
                    Score = a.Score,

                    Options = a.Question.QuestionType == QuestionType.MCQ
                        ? a.Question.Options.Select(o => new OptionDto
                        {
                            OptionText = o.OptionText,
                            IsCorrect = o.IsCorrect
                        }).ToList()
                        : new(),

                    //TestCases = a.Question.QuestionType == QuestionType.Coding
                    //    ? a.Question.TestCases.Select(t => new TestCaseDto
                    //    {
                    //        Input = t.Input,
                    //        ExpectedOutput = t.ExpectedOutput
                    //    }).ToList()
                    //    : new()
                }).ToList()
            };


            return new BaseResponse<SubmissionDto>
            {
                Status = true,
                Message = "Submission fetched successfully",
                Data = submissionDto
            };
        }

    }
}
