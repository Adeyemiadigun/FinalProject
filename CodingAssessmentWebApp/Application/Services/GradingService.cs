using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.ExternalServices;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.GradingStrategyInterfaces.Interfaces;
using Domain.Entitties;

namespace Application.Services
{
    public class GradingService(IGradingStrategyFactory gradingStrategyFactory, ISubmissionRepository _submissionRepository,IEmailService _emailService,IUnitOfWork _unitOfWork) : IGradingService
    {
        public async Task GradeSubmissionAndNotifyAsync(Guid submissionId, Guid studentId)
        {
            var submission = await _submissionRepository.GetFullSubmissionWithRelationsAsync(submissionId);
            if (submission == null)
            {
                throw new ApiException("Submission not found", 404, "SUBMISSION_NOT_FOUND", null);
            }
            if (submission.AnswerSubmissions == null || !submission.AnswerSubmissions.Any())
            {
                throw new ApiException("No answers found for this submission", 400, "NO_ANSWERS_FOUND", null);
            }
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
            submission.FeedBack = submission.TotalScore >= submission.Assessment.PassingScore ? "You Passed the assessment" : "You failed the assessment";
             _submissionRepository.Update(submission);
            await _unitOfWork.SaveChangesAsync();
            var student = submission.Student;
            var studentDto = new UserDto()
            {
                Email = student.Email,
                FullName = student.FullName,
            };
            await _emailService.SendResultEmailAsync(submission, studentDto);

        }
    }
}
